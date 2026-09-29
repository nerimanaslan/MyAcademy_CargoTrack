using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CargoTrack.DTO.DTOs.Cargos;
using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.Business.Services.Cargos
{
    public interface ICargoService
    {
        Task<string> CreateCargoAsync(CreateCargoDto dto, Guid? creatorUserId = null);
        Task UpdateStatusAsync(CargoStatusChangeDto dto, Guid? currentUserId = null, string? currentUserName = null);
        Task<bool> VerifyDeliveryCodeAndDeliverAsync(VerifyDeliveryCodeDto dto, Guid? currentUserId = null, string? currentUserName = null);
        Task RecordDeliveryExceptionAsync(CreateDeliveryExceptionDto dto, Guid? currentUserId = null, string? currentUserName = null);

        Task<PublicCargoTrackingDto?> GetPublicTrackingByCodeAsync(string trackCode);
        Task<List<PublicRecentShipmentDto>> GetRecentPublicShipmentsAsync(int count = 5);

        Task<CargoDetailDto?> GetCargoDetailByIdAsync(Guid id, Guid? currentUserId = null);
        Task<CargoDetailDto?> GetCargoDetailByTrackCodeAsync(string trackCode);

        Task<(List<ResultCargoDto> Items, int TotalCount)> GetAllCargosAsync(string? search = null, CargoStatus? status = null, int page = 1, int pageSize = 10);
        Task<(List<ResultCargoDto> Items, int TotalCount)> GetCargosByBranchAsync(Guid branchId, string? search = null, CargoStatus? status = null, int page = 1, int pageSize = 10);
        Task<(List<ResultCargoDto> Items, int TotalCount)> GetCargosByUserIdAsync(Guid userId, string? search = null, int page = 1, int pageSize = 10);

        Task DeleteAsync(Guid id, Guid? currentUserId = null, string? currentUserName = null);
    }
}
