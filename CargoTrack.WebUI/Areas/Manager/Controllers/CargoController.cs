using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CargoTrack.Business.Services.Branches;
using CargoTrack.Business.Services.Cargos;
using CargoTrack.Business.Services.Employees;
using CargoTrack.DTO.DTOs.Cargos;
using CargoTrack.Entity.Entities;
using CargoTrack.Entity.Entities.Enums;
using CargoTrack.WebUI.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CargoTrack.WebUI.Areas.Manager.Controllers
{
    [Area("Manager")]
    [Authorize(Roles = Roles.Manager)]
    public class CargoController(
        ICargoService _cargoService,
        IBranchService _branchService,
        IEmployeeService _employeeService,
        UserManager<AppUser> _userManager) : Controller
    {
        private async Task<Guid> GetManagerBranchIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.BranchId.HasValue == true)
            {
                return user.BranchId.Value;
            }
            return Guid.Empty;
        }

        public async Task<IActionResult> Index(string? search, CargoStatus? status, int page = 1)
        {
            var branchId = await GetManagerBranchIdAsync();
            if (branchId == Guid.Empty)
                return Forbid();

            const int pageSize = 10;
            var (items, totalCount) = await _cargoService.GetCargosByBranchAsync(branchId, search, status, page, pageSize);

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.TotalCount = totalCount;

            return View(items);
        }

        public async Task<IActionResult> Detail(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            CargoDetailDto? cargo;
            try
            {
                cargo = await _cargoService.GetCargoDetailByIdAsync(id, user?.Id);
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException)
            {
                return Forbid();
            }
            if (cargo == null)
            {
                TempData["error"] = "Kargo bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var branchId = await GetManagerBranchIdAsync();
            if (branchId == Guid.Empty)
                return Forbid();

            if (cargo.OriginBranchId != branchId &&
            cargo.DestinationBranchId != branchId)
            {
                return Forbid();
            }
            var employees = await _employeeService.GetByBranchIdAsync(branchId);
            ViewBag.Employees = new SelectList(employees, "Id", "FullName");

            return View(cargo);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(CargoStatusChangeDto dto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                Guid? guidUserId = Guid.TryParse(userId, out var parsed) ? parsed : null;

                var branchId = await GetManagerBranchIdAsync();
                if (branchId == Guid.Empty)
                    return Forbid();

                var cargo = await _cargoService.GetCargoDetailByIdAsync(dto.CargoId, guidUserId);
                if (cargo == null ||
                    (cargo.OriginBranchId != branchId &&
                     cargo.DestinationBranchId != branchId))
                {
                    return Forbid();
                }
                dto.BranchId = branchId;

                await _cargoService.UpdateStatusAsync(dto, guidUserId, User.Identity?.Name);
                TempData["success"] = "Kargo durumu başarıyla güncellendi.";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Detail), new { id = dto.CargoId });
        }

        public async Task<IActionResult> OutForDelivery(string? search)
        {
            var branchId = await GetManagerBranchIdAsync();
            if (branchId == Guid.Empty)
                return Forbid();

            var (items, _) = await _cargoService.GetCargosByBranchAsync(branchId, search, CargoStatus.OutForDelivery, 1, 100);

            var employees = await _employeeService.GetByBranchIdAsync(branchId);
            ViewBag.Employees = new SelectList(employees, "Id", "FullName");
            ViewBag.Search = search;

            return View(items);
        }

        [HttpPost]
        public async Task<IActionResult> VerifyDelivery(VerifyDeliveryCodeDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["error"] = "Teslimat kodu 6 haneli rakam olmalıdır.";
                    return RedirectToAction(nameof(OutForDelivery));
                }

                var branchId = await GetManagerBranchIdAsync();
                if (branchId == Guid.Empty)
                    return Forbid();

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                Guid? guidUserId = Guid.TryParse(userId, out var parsed) ? parsed : null;

                var cargo = await _cargoService.GetCargoDetailByIdAsync(dto.CargoId, guidUserId);

                if (cargo == null || cargo.DestinationBranchId != branchId)
                {
                    return Forbid();
                }

                await _cargoService.VerifyDeliveryCodeAndDeliverAsync(dto, guidUserId, User.Identity?.Name);
                TempData["success"] = "Teslimat kodu başarıyla doğrulandı ve kargo teslim edildi olarak kaydedildi.";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(OutForDelivery));
        }

        [HttpPost]
        public async Task<IActionResult> RecordException(CreateDeliveryExceptionDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["error"] = "Teslim edilememe sebebi seçilmelidir.";
                    return RedirectToAction(nameof(OutForDelivery));
                }

                var branchId = await GetManagerBranchIdAsync();
                if (branchId == Guid.Empty)
                    return Forbid();

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                Guid? guidUserId = Guid.TryParse(userId, out var parsed) ? parsed : null;

                var cargo = await _cargoService.GetCargoDetailByIdAsync(dto.CargoId, guidUserId);

                if (cargo == null || cargo.DestinationBranchId != branchId)
                {
                    return Forbid();
                }

                await _cargoService.RecordDeliveryExceptionAsync(dto, guidUserId, User.Identity?.Name);
                TempData["success"] = "Teslimat hatası kaydedildi. Kargo durumu güncellendi.";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(OutForDelivery));
        }
    }
}
