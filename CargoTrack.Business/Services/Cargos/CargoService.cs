using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CargoTrack.Business.Services.AuditLogs;
using CargoTrack.Business.Services.CargoPrices;
using CargoTrack.DataAccess.Repositories.Branches;
using CargoTrack.DataAccess.Repositories.CargoMovements;
using CargoTrack.DataAccess.Repositories.Cargos;
using CargoTrack.DataAccess.Repositories.Deliveries;
using CargoTrack.DataAccess.Repositories.DeliveryExceptions;
using CargoTrack.DataAccess.Repositories.Employees;
using CargoTrack.DataAccess.Repositories.TransferCenters;
using CargoTrack.DTO.DTOs.CargoPriceDtos;
using CargoTrack.DTO.DTOs.Cargos;
using CargoTrack.Entity.Entities;
using CargoTrack.Entity.Entities.Enums;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace CargoTrack.Business.Services.Cargos
{
    public class CargoService(
        ICargoRepository _cargoRepository,
        ICargoMovementRepository _movementRepository,
        IBranchRepository _branchRepository,
        ITransferCenterRepository _transferCenterRepository,
        IEmployeeRepository _employeeRepository,
        IDeliveryRepository _deliveryRepository,
        IDeliveryExceptionRepository _exceptionRepository,
        ICargoPricingService _pricingService,
        IAuditLogService _auditLogService,
        UserManager<AppUser> _userManager) : ICargoService

    {
        private static readonly Random _random = new();

        public async Task<string> CreateCargoAsync(CreateCargoDto dto, Guid? creatorUserId = null)
        {
            // 1. Generate Unique Track Code: MYC-2026-XXXXXX
            string trackCode;
            do
            {
                int randomNum = _random.Next(100000, 999999);
                trackCode = $"MYC-{DateTime.Now.Year}-{randomNum}";
            } while (await _cargoRepository.AnyAsync(c => c.TrackCode == trackCode));

            // 2. Dynamic Price & Desi Calculation
            var priceResult = await _pricingService.CalculatePriceAsync(new CalculatePriceDto
            {
                Weight = dto.Weight,
                Width = dto.Width,
                Height = dto.Height,
                Length = dto.Length,
                OriginBranchId = dto.OriginBranchId,
                DestinationBranchId = dto.DestinationBranchId,
                CargoType = dto.CargoType
            });

            decimal finalPrice = dto.CustomPrice ?? priceResult.TotalPrice;

            // 3. SLA / Estimated Delivery Date
            DateTime shipmentDate = DateTime.Now;
            DateTime estimatedDelivery = shipmentDate.AddDays(priceResult.EstimatedDays);

            // 4. Create Cargo Entity
            var cargo = new Cargo
            {
                TrackCode = trackCode,
                ShipmentDate = shipmentDate,
                EstimatedDeliveryDate = estimatedDelivery,
                Weight = dto.Weight,
                Width = dto.Width,
                Height = dto.Height,
                Length = dto.Length,
                Desi = priceResult.Desi,
                Price = finalPrice,
                CargoType = dto.CargoType,
                CargoStatus = CargoStatus.Created,
                SenderId = dto.SenderId,
                ReceiverId = dto.ReceiverId,
                ReceiverName = dto.ReceiverName,
                ReceiverPhone = dto.ReceiverPhone,
                ReceiverAddress = dto.ReceiverAddress,
                OriginBranchId = dto.OriginBranchId,
                DestinationBranchId = dto.DestinationBranchId,
                CurrentBranchId = dto.OriginBranchId
            };

            await _cargoRepository.CreateAsync(cargo);

            // 5. Initial Movement Log
            var originBranch = await _branchRepository.GetByIdAsync(dto.OriginBranchId);
            string originLocationName = originBranch?.Name ?? "Gönderici Şubesi";

            var movement = new CargoMovement
            {
                CargoId = cargo.Id,
                PreviousStatus = null,
                NewStatus = CargoStatus.Created,
                Timestamp = DateTime.Now,
                LocationName = originLocationName,
                BranchId = dto.OriginBranchId,
                Description = "Kargo kabulü yapıldı, gönderi kaydı oluşturuldu."
            };
            await _movementRepository.CreateAsync(movement);

            // 6. Audit Log
            await _auditLogService.LogAsync(
                actionType: "Create",
                entityName: "Cargo",
                entityId: cargo.Id.ToString(),
                oldValue: null,
                newValue: cargo.CargoStatus.ToString(),
                description: $"Yeni kargo oluşturuldu. Takip No: {cargo.TrackCode}, Desi: {cargo.Desi}, Fiyat: {cargo.Price} TL",
                userId: creatorUserId
            );

            return cargo.TrackCode;
        }

        public async Task UpdateStatusAsync(CargoStatusChangeDto dto, Guid? currentUserId = null, string? currentUserName = null)
        {
            var cargo = await _cargoRepository.GetByIdAsync(dto.CargoId);
            if (cargo == null)
                throw new ValidationException("Kargo bulunamadı.");
            await CheckManagerBranchAsync(cargo, currentUserId);

            if (dto.BranchId.HasValue && await _branchRepository.GetByIdAsync(dto.BranchId.Value) == null)
                throw new ValidationException("İşlem şubesi bulunamadı.");

            if (dto.TransferCenterId.HasValue && await _transferCenterRepository.GetByIdAsync(dto.TransferCenterId.Value) == null)
                throw new ValidationException("Transfer merkezi bulunamadı.");

            CheckStatusLocation(cargo, dto.NewStatus, dto.BranchId);

            if (dto.NewStatus == CargoStatus.ReturnProcess && cargo.FailedDeliveryAttempts < 3)
                throw new ValidationException("İade süreci için üç başarısız teslimat denemesi gerekir.");

            var oldStatus = cargo.CargoStatus;
            var newStatus = dto.NewStatus;
            if (newStatus == CargoStatus.DeliveryFailed)
                throw new ValidationException("Başarısız teslimatı hata kaydı oluşturarak giriniz.");
            // Validate state machine transition rules
            ValidateStatusTransition(oldStatus, newStatus);

            cargo.CargoStatus = newStatus;
            string movementDescription = string.IsNullOrEmpty(dto.Description)
                ? GetDefaultDescriptionForStatus(newStatus)
                : dto.Description;

            // Handle transition to OutForDelivery: Generate 6-digit pin code
            if (newStatus == CargoStatus.OutForDelivery)
            {
                cargo.DeliveryPinCode = _random.Next(100000, 999999).ToString();

            }

            // Handle transition to Delivered: Must verify pin code
            if (newStatus == CargoStatus.Delivered)
            {
                if (string.IsNullOrEmpty(cargo.DeliveryPinCode) ||
                string.IsNullOrEmpty(dto.DeliveryPinCode) ||
                dto.DeliveryPinCode.Trim() != cargo.DeliveryPinCode.Trim())
                {
                    throw new ValidationException("Teslimat kodu doğrulanamadı! Teslimat güvenlik kodu girilmeden kargo teslim edilemez.");
                }

                cargo.ArrivalDate = DateTime.Now;
                cargo.ReceivedBy = dto.ReceivedBy ?? cargo.ReceiverName;
                cargo.DeliveredByEmployeeId = dto.EmployeeId;

                // Create Delivery record
                await _deliveryRepository.CreateAsync(new Delivery
                {
                    CargoId = cargo.Id,
                    DeliveryDate = DateTime.Now,
                    ReceivedBy = cargo.ReceivedBy,
                    DeliveredByEmployeeId = dto.EmployeeId,
                    DeliveryPinCodeVerified = true,
                    Notes = dto.Description
                });
            }

            // Update Current Location
            if (dto.BranchId.HasValue)
            {
                cargo.CurrentBranchId = dto.BranchId;
                cargo.CurrentTransferCenterId = null;
            }
            else if (dto.TransferCenterId.HasValue)
            {
                cargo.CurrentTransferCenterId = dto.TransferCenterId;
                cargo.CurrentBranchId = null;
            }

            await _cargoRepository.UpdateAsync(cargo);

            // Record Movement
            var movement = new CargoMovement
            {
                CargoId = cargo.Id,
                PreviousStatus = oldStatus,
                NewStatus = newStatus,
                Timestamp = DateTime.Now,
                Description = movementDescription,
                LocationName = dto.LocationName,
                BranchId = dto.BranchId,
                TransferCenterId = dto.TransferCenterId,
                EmployeeId = dto.EmployeeId
            };
            await _movementRepository.CreateAsync(movement);

            // Record Audit Log
            await _auditLogService.LogAsync(
                actionType: "StatusChange",
                entityName: "Cargo",
                entityId: cargo.Id.ToString(),
                oldValue: oldStatus.ToString(),
                newValue: newStatus.ToString(),
                description: $"Kargo durumu güncellendi: {oldStatus} -> {newStatus}. Konum: {dto.LocationName}. {movementDescription}",
                userId: currentUserId,
                userName: currentUserName
            );
        }

        public async Task<bool> VerifyDeliveryCodeAndDeliverAsync(VerifyDeliveryCodeDto dto, Guid? currentUserId = null, string? currentUserName = null)
        {
            var cargo = await _cargoRepository.GetByIdAsync(dto.CargoId);
            if (cargo == null)
                throw new ValidationException("Kargo bulunamadı.");

            await CheckManagerBranchAsync(cargo, currentUserId);

            if (cargo.CargoStatus != CargoStatus.OutForDelivery && cargo.CargoStatus != CargoStatus.AtDestinationBranch)
                throw new ValidationException("Kargo henüz teslimat aşamasında değil (Varış Şubesinde veya Dağıtımda olmalıdır).");
            if (string.IsNullOrEmpty(cargo.DeliveryPinCode) || cargo.DeliveryPinCode.Trim() != dto.DeliveryCode.Trim())
                throw new ValidationException("Teslimat kodu geçersiz.");
            var oldStatus = cargo.CargoStatus;
            cargo.CargoStatus = CargoStatus.Delivered;
            cargo.ArrivalDate = DateTime.Now;
            cargo.ReceivedBy = string.IsNullOrWhiteSpace(dto.ReceivedBy) ? cargo.ReceiverName : dto.ReceivedBy;
            cargo.DeliveredByEmployeeId = dto.EmployeeId;

            await _cargoRepository.UpdateAsync(cargo);

            // Add Delivery Record
            await _deliveryRepository.CreateAsync(new Delivery
            {
                CargoId = cargo.Id,
                DeliveryDate = DateTime.Now,
                ReceivedBy = cargo.ReceivedBy,
                ReceiverRelationship = dto.ReceiverRelationship ?? "Kendisi",
                DeliveredByEmployeeId = dto.EmployeeId,
                DeliveryPinCodeVerified = true,
                Notes = dto.Notes
            });

            // Add Movement
            await _movementRepository.CreateAsync(new CargoMovement
            {
                CargoId = cargo.Id,
                PreviousStatus = oldStatus,
                NewStatus = CargoStatus.Delivered,
                Timestamp = DateTime.Now,
                Description = $"Kargo teslim edildi. Teslim Alan: {cargo.ReceivedBy} ({dto.ReceiverRelationship ?? "Kendisi"}). Güvenlik kodu doğrulandı.",
                LocationName = cargo.DestinationBranch?.Name ?? "Varış Şubesi",
                BranchId = cargo.DestinationBranchId,
                EmployeeId = dto.EmployeeId
            });

            // Audit Log
            await _auditLogService.LogAsync(
                actionType: "Delivery",
                entityName: "Cargo",
                entityId: cargo.Id.ToString(),
                oldValue: oldStatus.ToString(),
                newValue: CargoStatus.Delivered.ToString(),
                description: $"Kargo teslimatı kod doğrulanarak tamamlandı. Teslim Alan: {cargo.ReceivedBy}",
                userId: currentUserId,
                userName: currentUserName
            );

            return true;
        }

        public async Task RecordDeliveryExceptionAsync(CreateDeliveryExceptionDto dto, Guid? currentUserId = null, string? currentUserName = null)
        {
            var cargo = await _cargoRepository.GetByIdAsync(dto.CargoId);
            if (cargo == null)
                throw new ValidationException("Kargo bulunamadı.");
            await CheckManagerBranchAsync(cargo, currentUserId);
            if (cargo.CargoStatus != CargoStatus.OutForDelivery)
                throw new ValidationException("Yalnızca dağıtımdaki kargo için başarısız teslimat kaydedilebilir.");
            cargo.FailedDeliveryAttempts++;
            int attemptNum = cargo.FailedDeliveryAttempts;

            // Create DeliveryException record
            var exception = new DeliveryException
            {
                CargoId = cargo.Id,
                ExceptionDate = DateTime.Now,
                Reason = dto.Reason,
                Notes = dto.Notes,
                AttemptNumber = attemptNum,
                RecordedByEmployeeId = dto.EmployeeId
            };
            await _exceptionRepository.CreateAsync(exception);

            var oldStatus = cargo.CargoStatus;
            CargoStatus newStatus;
            string movementDescription;

            // If 3 failed attempts, initiate return process according to case rules:
            // "üç başarısız deneme -> İade Sürecinde -> Göndericiye İade Edildi."
            if (cargo.FailedDeliveryAttempts >= 3)
            {
                newStatus = CargoStatus.ReturnProcess;
                movementDescription = $"3 başarısız teslimat denemesi ({dto.Reason}) nedeniyle kargo İADE SÜRECİNE alındı.";
            }
            else
            {
                newStatus = CargoStatus.DeliveryFailed;
                movementDescription = $"Teslimat denemesi #{attemptNum} başarısız oldu: {dto.Reason}. Kargo yeniden dağıtıma çıkarılacak.";
            }

            cargo.CargoStatus = newStatus;
            await _cargoRepository.UpdateAsync(cargo);

            // Movement
            await _movementRepository.CreateAsync(new CargoMovement
            {
                CargoId = cargo.Id,
                PreviousStatus = oldStatus,
                NewStatus = newStatus,
                Timestamp = DateTime.Now,
                Description = movementDescription,
                LocationName = cargo.DestinationBranch?.Name ?? "Dağıtım Bölgesi",
                BranchId = cargo.DestinationBranchId,
                EmployeeId = dto.EmployeeId
            });

            // Audit
            await _auditLogService.LogAsync(
                actionType: "DeliveryException",
                entityName: "Cargo",
                entityId: cargo.Id.ToString(),
                oldValue: oldStatus.ToString(),
                newValue: newStatus.ToString(),
                description: $"Teslimat hatası kaydedildi (#{attemptNum}). Neden: {dto.Reason}. Son durum: {newStatus}",
                userId: currentUserId,
                userName: currentUserName
            );
        }

        public async Task<PublicCargoTrackingDto?> GetPublicTrackingByCodeAsync(string trackCode)
        {
            var list = await _cargoRepository.GetListAsync(
                predicate: c => c.TrackCode == trackCode,
                include: q => q.Include(c => c.OriginBranch).ThenInclude(b => b.City)
                               .Include(c => c.DestinationBranch).ThenInclude(b => b.City)
                               .Include(c => c.Sender)
                               .Include(c => c.Receiver)
                               .Include(c => c.Movements)
            );

            var cargo = list.FirstOrDefault();
            if (cargo == null) return null;

            var movements = cargo.Movements.OrderBy(m => m.Timestamp).ToList();

            var result = new PublicCargoTrackingDto
            {
                TrackCode = cargo.TrackCode,
                Status = cargo.CargoStatus,
                StatusName = GetStatusDisplayName(cargo.CargoStatus),
                StatusBadgeClass = GetStatusBadgeClass(cargo.CargoStatus),
                StatusIcon = GetStatusIcon(cargo.CargoStatus),
                ShipmentDateFormatted = cargo.ShipmentDate.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("tr-TR")),
                EstimatedDeliveryFormatted = cargo.EstimatedDeliveryDate.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("tr-TR")),
                EstimatedDeliveryTitle = cargo.CargoStatus == CargoStatus.Delivered
                    ? $"Teslim Edildi: {cargo.ArrivalDate?.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("tr-TR"))}"
                    : (cargo.EstimatedDeliveryDate.Date == DateTime.Today ? "Tahmini Teslimat: Bugün" : $"Tahmini Teslimat: {cargo.EstimatedDeliveryDate:dd MMMM yyyy}"),
                EstimatedTimeWindow = "14:00 - 18:00",
                ProgressPercentage = CalculateProgressPercentage(cargo.CargoStatus),
                OriginCity = cargo.OriginBranch?.City?.Name ?? "İstanbul",
                OriginDistrict = cargo.OriginBranch?.Name ?? "Ataşehir",
                DestinationCity = cargo.DestinationBranch?.City?.Name ?? "Ankara",
                DestinationDistrict = cargo.DestinationBranch?.Name ?? "Çankaya",
                MaskedSenderName = MaskName(cargo.Sender != null ? cargo.Sender.FullName : "Gönderici"),
                SenderLocation = $"{cargo.OriginBranch?.City?.Name} / {cargo.OriginBranch?.Name}",
                MaskedReceiverName = MaskName(!string.IsNullOrEmpty(cargo.ReceiverName) ? cargo.ReceiverName : (cargo.Receiver?.FullName ?? "Alıcı")),
                ReceiverLocation = $"{cargo.DestinationBranch?.City?.Name} / {cargo.DestinationBranch?.Name}",
                WeightFormatted = $"{cargo.Weight:F1} kg / {cargo.Desi:F1} desi",
                CargoTypeDescription = GetCargoTypeDisplayName(cargo.CargoType),
                OriginBranchName = cargo.OriginBranch?.Name ?? "Çıkış Şubesi",
                DestinationBranchName = cargo.DestinationBranch?.Name ?? "Varış Şubesi"
            };

            // Map movements to public timeline items
            for (int i = 0; i < movements.Count; i++)
            {
                var m = movements[i];
                bool isLast = i == movements.Count - 1;
                result.Movements.Add(new PublicMovementItemDto
                {
                    Title = GetStatusDisplayName(m.NewStatus),
                    Location = m.LocationName,
                    DateFormatted = m.Timestamp.ToString("dd MMM", new System.Globalization.CultureInfo("tr-TR")),
                    TimeFormatted = m.Timestamp.ToString("HH:mm"),
                    IsCompleted = true,
                    IsCurrent = isLast,
                    IconName = isLast && cargo.CargoStatus == CargoStatus.OutForDelivery ? "local_shipping" : "check",
                    CircleColorClass = isLast && cargo.CargoStatus == CargoStatus.OutForDelivery ? "bg-secondary-container text-white" : "bg-green-500 text-white"
                });
            }

            return result;
        }

        public async Task<List<PublicRecentShipmentDto>> GetRecentPublicShipmentsAsync(int count = 5)
        {
            var list = await _cargoRepository.GetListAsync(
                include: q => q.Include(c => c.OriginBranch).ThenInclude(b => b.City)
                               .Include(c => c.DestinationBranch).ThenInclude(b => b.City),
                orderBy: q => q.OrderByDescending(c => c.CreatedDate),
                take: count
            );

            return list.Select(c => new PublicRecentShipmentDto
            {
                TrackCode = c.TrackCode,
                StatusText = GetStatusDisplayName(c.CargoStatus),
                StatusBadgeClass = GetStatusBadgeClass(c.CargoStatus),
                StatusIcon = GetStatusIcon(c.CargoStatus),
                OriginCity = c.OriginBranch?.City?.Name ?? "İstanbul",
                DestinationCity = c.DestinationBranch?.City?.Name ?? "Ankara",
                DateLabel = c.CargoStatus == CargoStatus.Delivered ? "Teslim Tarihi" : "Tahmini Teslimat",
                DateFormatted = (c.CargoStatus == CargoStatus.Delivered && c.ArrivalDate.HasValue ? c.ArrivalDate.Value : c.EstimatedDeliveryDate).ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("tr-TR")),
                BorderColorClass = GetStatusBorderClass(c.CargoStatus)
            }).ToList();
        }

        public async Task<CargoDetailDto?> GetCargoDetailByIdAsync(Guid id, Guid? currentUserId = null)
        {
            var list = await _cargoRepository.GetListAsync(
                predicate: c => c.Id == id,
                include: q => q.Include(c => c.OriginBranch).ThenInclude(b => b.City)
                               .Include(c => c.DestinationBranch).ThenInclude(b => b.City)
                               .Include(c => c.Sender)
                               .Include(c => c.Receiver)
                               .Include(c => c.Movements)
                               .Include(c => c.Exceptions)
            );

            var cargo = list.FirstOrDefault();
            if (cargo == null) return null;

            if (currentUserId.HasValue)
                await CheckCargoAccessAsync(cargo, currentUserId.Value);

            return MapToDetailDto(cargo);
        }

        public async Task<CargoDetailDto?> GetCargoDetailByTrackCodeAsync(string trackCode)
        {
            var list = await _cargoRepository.GetListAsync(
                predicate: c => c.TrackCode == trackCode,
                include: q => q.Include(c => c.OriginBranch).ThenInclude(b => b.City)
                               .Include(c => c.DestinationBranch).ThenInclude(b => b.City)
                               .Include(c => c.Sender)
                               .Include(c => c.Receiver)
                               .Include(c => c.Movements)
                               .Include(c => c.Exceptions)
            );

            var cargo = list.FirstOrDefault();
            if (cargo == null) return null;

            return MapToDetailDto(cargo);
        }

        public async Task<(List<ResultCargoDto> Items, int TotalCount)> GetAllCargosAsync(
            string? search = null, CargoStatus? status = null, int page = 1, int pageSize = 10)
        {
            var query = _cargoRepository.GetQueryable()
                .Include(c => c.OriginBranch).ThenInclude(b => b.City)
                .Include(c => c.DestinationBranch).ThenInclude(b => b.City)
                .Include(c => c.Sender)
                .Include(c => c.Receiver)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim();
                query = query.Where(c => c.TrackCode.Contains(search) || c.ReceiverName.Contains(search) || (c.Sender != null && (c.Sender.FirstName.Contains(search) || c.Sender.LastName.Contains(search))));
            }

            if (status.HasValue)
            {
                query = query.Where(c => c.CargoStatus == status.Value);
            }

            int totalCount = await query.CountAsync();
            var cargos = await query
                .OrderByDescending(c => c.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (cargos.Select(MapToResultDto).ToList(), totalCount);
        }

        public async Task<(List<ResultCargoDto> Items, int TotalCount)> GetCargosByBranchAsync(
            Guid branchId, string? search = null, CargoStatus? status = null, int page = 1, int pageSize = 10)
        {
            var query = _cargoRepository.GetQueryable()
                .Include(c => c.OriginBranch).ThenInclude(b => b.City)
                .Include(c => c.DestinationBranch).ThenInclude(b => b.City)
                .Include(c => c.Sender)
                .Include(c => c.Receiver)
                .Where(c => c.OriginBranchId == branchId || c.DestinationBranchId == branchId || c.CurrentBranchId == branchId);

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim();
                query = query.Where(c => c.TrackCode.Contains(search) || c.ReceiverName.Contains(search));
            }

            if (status.HasValue)
            {
                query = query.Where(c => c.CargoStatus == status.Value);
            }

            int totalCount = await query.CountAsync();
            var cargos = await query
                .OrderByDescending(c => c.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (cargos.Select(MapToResultDto).ToList(), totalCount);
        }

        public async Task<(List<ResultCargoDto> Items, int TotalCount)> GetCargosByUserIdAsync(
            Guid userId, string? search = null, int page = 1, int pageSize = 10)
        {
            var query = _cargoRepository.GetQueryable()
                .Include(c => c.OriginBranch).ThenInclude(b => b.City)
                .Include(c => c.DestinationBranch).ThenInclude(b => b.City)
                .Include(c => c.Sender)
                .Include(c => c.Receiver)
                .Where(c => c.SenderId == userId || c.ReceiverId == userId);

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim();
                query = query.Where(c => c.TrackCode.Contains(search) || c.ReceiverName.Contains(search));
            }

            int totalCount = await query.CountAsync();
            var cargos = await query
                .OrderByDescending(c => c.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (cargos.Select(MapToResultDto).ToList(), totalCount);
        }

        public async Task DeleteAsync(Guid id, Guid? currentUserId = null, string? currentUserName = null)
        {
            var cargo = await _cargoRepository.GetByIdAsync(id);
            if (cargo == null)
                throw new ValidationException("Kargo bulunamadı.");

            await _cargoRepository.DeleteAsync(cargo);

            await _auditLogService.LogAsync(
                actionType: "Delete",
                entityName: "Cargo",
                entityId: cargo.Id.ToString(),
                oldValue: cargo.CargoStatus.ToString(),
                newValue: "Deleted",
                description: $"Kargo silindi (Soft Delete). Takip No: {cargo.TrackCode}",
                userId: currentUserId,
                userName: currentUserName
            );
        }
        private async Task CheckCargoAccessAsync(Cargo cargo, Guid currentUserId)
        {
            var user = await _userManager.FindByIdAsync(currentUserId.ToString());
            if (user == null)
                throw new ValidationException("Kullanıcı bulunamadı.");

            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return;

            if (await _userManager.IsInRoleAsync(user, "Manager"))
            {
                if (!user.BranchId.HasValue ||
                    (cargo.OriginBranchId != user.BranchId.Value &&
                     cargo.DestinationBranchId != user.BranchId.Value &&
                     cargo.CurrentBranchId != user.BranchId.Value))
                    throw new ValidationException("Bu kargo şubenize ait değil.");

                return;
            }

            if (cargo.SenderId != currentUserId && cargo.ReceiverId != currentUserId)
                throw new ValidationException("Bu kargo size ait değil.");
        }

        private async Task CheckManagerBranchAsync(Cargo cargo, Guid? currentUserId)
        {
            if (!currentUserId.HasValue)
                throw new ValidationException("İşlemi yapan kullanıcı bulunamadı.");

            var user = await _userManager.FindByIdAsync(currentUserId.Value.ToString());
            if (user == null)
                throw new ValidationException("Kullanıcı bulunamadı.");

            if (!await _userManager.IsInRoleAsync(user, "Manager"))
                return;
            if (!user.BranchId.HasValue ||
                (cargo.OriginBranchId != user.BranchId.Value &&
                 cargo.DestinationBranchId != user.BranchId.Value &&
                 cargo.CurrentBranchId != user.BranchId.Value))
                throw new ValidationException("Bu kargo şubenize ait değil.");
        }

        private static void CheckStatusLocation(Cargo cargo, CargoStatus newStatus, Guid? branchId)
        {
            if (newStatus == CargoStatus.AtOriginBranch && branchId.HasValue && branchId.Value != cargo.OriginBranchId)
                throw new ValidationException("Kargo çıkış şubesi dışında gönderici şubesinde gösterilemez.");

            if ((newStatus == CargoStatus.AtDestinationBranch || newStatus == CargoStatus.OutForDelivery || newStatus == CargoStatus.Delivered || newStatus == CargoStatus.DeliveryFailed) &&
                branchId.HasValue && branchId.Value != cargo.DestinationBranchId)
                throw new ValidationException("Bu işlem varış şubesinde yapılmalıdır.");

        }
        #region Helper Methods

        private static void ValidateStatusTransition(CargoStatus oldStatus, CargoStatus newStatus)
        {
            if (oldStatus == newStatus && oldStatus != CargoStatus.InTransferCenter)
                throw new ValidationException("Kargo zaten bu durumda.");

            // Finished terminal states cannot transition further
            if (oldStatus == CargoStatus.Delivered)
                throw new ValidationException("Teslim edilmiş bir kargonun durumu değiştirilemez.");

            if (oldStatus == CargoStatus.ReturnedToSender)
                throw new ValidationException("Göndericiye iade edilmiş bir kargonun durumu değiştirilemez.");

            if (oldStatus == CargoStatus.Canceled)
                throw new ValidationException("İptal edilmiş bir kargonun durumu değiştirilemez.");

            // Allow cancel from active states
            if (newStatus == CargoStatus.Canceled) return;

            // Specific workflow transitions
            bool isValid = (oldStatus, newStatus) switch
            {
                (CargoStatus.Created, CargoStatus.AtOriginBranch) => true,
                (CargoStatus.AtOriginBranch, CargoStatus.InTransferCenter) => true,
                (CargoStatus.AtOriginBranch, CargoStatus.AtDestinationBranch) => true,
                (CargoStatus.InTransferCenter, CargoStatus.InTransferCenter) => true, // intermediate transfer center
                (CargoStatus.InTransferCenter, CargoStatus.AtDestinationBranch) => true,
                (CargoStatus.AtDestinationBranch, CargoStatus.OutForDelivery) => true,
                (CargoStatus.OutForDelivery, CargoStatus.Delivered) => true,
                (CargoStatus.OutForDelivery, CargoStatus.DeliveryFailed) => true,
                (CargoStatus.DeliveryFailed, CargoStatus.OutForDelivery) => true, // re-delivery
                (CargoStatus.DeliveryFailed, CargoStatus.ReturnProcess) => true,
                (CargoStatus.ReturnProcess, CargoStatus.ReturnedToSender) => true,
                _ => false
            };

            if (!isValid)
            {
                throw new ValidationException($"Geçersiz durum geçişi: '{GetStatusDisplayName(oldStatus)}' durumundan '{GetStatusDisplayName(newStatus)}' durumuna doğrudan geçilemez.");
            }
        }

        private static string GetDefaultDescriptionForStatus(CargoStatus status) => status switch
        {
            CargoStatus.Created => "Kargo oluşturuldu.",
            CargoStatus.AtOriginBranch => "Kargo gönderici şubesine ulaştı ve kabul edildi.",
            CargoStatus.InTransferCenter => "Kargo transfer merkezine ulaştı.",
            CargoStatus.AtDestinationBranch => "Kargo varış şubesine ulaştı.",
            CargoStatus.OutForDelivery => "Kargo dağıtıma çıkarıldı.",
            CargoStatus.Delivered => "Kargo alıcıya başarıyla teslim edildi.",
            CargoStatus.DeliveryFailed => "Kargo teslim edilemedi.",
            CargoStatus.ReturnProcess => "Kargo iade sürecine alındı.",
            CargoStatus.ReturnedToSender => "Kargo göndericiye iade edildi.",
            CargoStatus.Canceled => "Kargo gönderisi iptal edildi.",
            _ => "Durum güncellendi."
        };

        public static string GetStatusDisplayName(CargoStatus status) => status switch
        {
            CargoStatus.Created => "Oluşturuldu",
            CargoStatus.AtOriginBranch => "Gönderici Şubesinde",
            CargoStatus.InTransferCenter => "Transfer Merkezinde",
            CargoStatus.AtDestinationBranch => "Varış Şubesinde",
            CargoStatus.OutForDelivery => "Dağıtıma Çıktı",
            CargoStatus.Delivered => "Teslim Edildi",
            CargoStatus.DeliveryFailed => "Teslim Edilemedi",
            CargoStatus.ReturnProcess => "İade Sürecinde",
            CargoStatus.ReturnedToSender => "Göndericiye İade Edildi",
            CargoStatus.Canceled => "İptal Edildi",
            _ => status.ToString()
        };

        private static string GetStatusBadgeClass(CargoStatus status) => status switch
        {
            CargoStatus.Created => "bg-gray-100 text-gray-800",
            CargoStatus.AtOriginBranch => "bg-blue-100 text-blue-800",
            CargoStatus.InTransferCenter => "bg-[#fff3e0] text-[#e65100]",
            CargoStatus.AtDestinationBranch => "bg-indigo-100 text-indigo-800",
            CargoStatus.OutForDelivery => "bg-surface-container-high text-primary-container",
            CargoStatus.Delivered => "bg-[#e8f5e9] text-[#2e7d32]",
            CargoStatus.DeliveryFailed => "bg-red-100 text-red-800",
            CargoStatus.ReturnProcess => "bg-orange-100 text-orange-800",
            CargoStatus.ReturnedToSender => "bg-purple-100 text-purple-800",
            CargoStatus.Canceled => "bg-gray-200 text-gray-600",
            _ => "bg-gray-100 text-gray-800"
        };

        private static string GetStatusBorderClass(CargoStatus status) => status switch
        {
            CargoStatus.OutForDelivery => "border-l-surface-tint",
            CargoStatus.InTransferCenter => "border-l-secondary-container",
            CargoStatus.Delivered => "border-l-[#4caf50]",
            CargoStatus.DeliveryFailed => "border-l-red-500",
            CargoStatus.ReturnProcess => "border-l-orange-500",
            _ => "border-l-primary"
        };

        private static string GetStatusIcon(CargoStatus status) => status switch
        {
            CargoStatus.Created => "inventory_2",
            CargoStatus.AtOriginBranch => "store",
            CargoStatus.InTransferCenter => "swap_horiz",
            CargoStatus.AtDestinationBranch => "domain",
            CargoStatus.OutForDelivery => "local_shipping",
            CargoStatus.Delivered => "check_circle",
            CargoStatus.DeliveryFailed => "warning",
            CargoStatus.ReturnProcess => "replay",
            CargoStatus.ReturnedToSender => "assignment_return",
            _ => "package"
        };

        private static int CalculateProgressPercentage(CargoStatus status) => status switch
        {
            CargoStatus.Created => 15,
            CargoStatus.AtOriginBranch => 30,
            CargoStatus.InTransferCenter => 50,
            CargoStatus.AtDestinationBranch => 70,
            CargoStatus.OutForDelivery => 85,
            CargoStatus.Delivered => 100,
            CargoStatus.DeliveryFailed => 80,
            CargoStatus.ReturnProcess => 50,
            CargoStatus.ReturnedToSender => 100,
            CargoStatus.Canceled => 0,
            _ => 10
        };

        private static string MaskName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "***";
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", parts.Select(p => p.Length > 1 ? p[0] + new string('*', p.Length - 1) : p));
        }

        private static string GetCargoTypeDisplayName(CargoType type) => type switch
        {
            CargoType.Standard => "Standart Gönderi",
            CargoType.Express => "Hızlı / Express Gönderi",
            CargoType.Fragile => "Hassas / Kırılabilir Gönderi",
            CargoType.Heavy => "Ağır Yük / Palet",
            CargoType.SameDay => "Aynı Gün Teslimat",
            _ => type.ToString()
        };

        private static ResultCargoDto MapToResultDto(Cargo c)
        {
            var dto = c.Adapt<ResultCargoDto>();
            dto.SenderName = c.Sender?.FullName ?? "Gönderici";
            dto.ReceiverName = !string.IsNullOrEmpty(c.ReceiverName) ? c.ReceiverName : (c.Receiver?.FullName ?? "Alıcı");
            dto.OriginBranchName = c.OriginBranch?.Name ?? "Çıkış Şubesi";
            dto.DestinationBranchName = c.DestinationBranch?.Name ?? "Varış Şubesi";
            dto.CurrentLocationName = c.CurrentBranch?.Name ?? c.CurrentTransferCenter?.Name ?? c.OriginBranch?.Name ?? "Operasyon Merkezi";
            return dto;
        }

        private static CargoDetailDto MapToDetailDto(Cargo c)
        {
            var dto = c.Adapt<CargoDetailDto>();
            dto.SenderName = c.Sender?.FullName ?? "Gönderici";
            dto.SenderPhone = c.Sender?.PhoneNumber ?? "";
            dto.SenderEmail = c.Sender?.Email ?? "";
            dto.ReceiverName = !string.IsNullOrEmpty(c.ReceiverName) ? c.ReceiverName : (c.Receiver?.FullName ?? "Alıcı");
            dto.OriginBranchName = c.OriginBranch?.Name ?? "";
            dto.OriginCityName = c.OriginBranch?.City?.Name ?? "";
            dto.DestinationBranchName = c.DestinationBranch?.Name ?? "";
            dto.DestinationCityName = c.DestinationBranch?.City?.Name ?? "";
            dto.CurrentLocationName = c.CurrentBranch?.Name ?? c.CurrentTransferCenter?.Name ?? "";
            dto.Movements = c.Movements.OrderBy(m => m.Timestamp).Select(m =>
            {
                var movement = m.Adapt<CargoMovementDto>();
                movement.EmployeeName = m.Employee?.FullName;
                return movement;
            }).ToList();
            dto.Exceptions = c.Exceptions.OrderBy(e => e.ExceptionDate).Select(e =>
            {
                var exception = e.Adapt<DeliveryExceptionDetailDto>();
                exception.EmployeeName = e.RecordedByEmployee?.FullName;
                return exception;
            }).ToList();
            return dto;
        }

        #endregion
    }
}
