using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CargoTrack.Business.Services.Branches;
using CargoTrack.Business.Services.CargoPrices;
using CargoTrack.Business.Services.Cargos;
using CargoTrack.Business.Services.TransferCenters;
using CargoTrack.DTO.DTOs.Cargos;
using CargoTrack.Entity.Entities;
using CargoTrack.Entity.Entities.Enums;
using CargoTrack.WebUI.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CargoTrack.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class CargoController(
        ICargoService _cargoService,
        IBranchService _branchService,
        ITransferCenterService _transferCenterService,
        UserManager<AppUser> _userManager) : Controller
    {
        public async Task<IActionResult> Index(string? search, CargoStatus? status, int page = 1)
        {
            const int pageSize = 10;
            var (items, totalCount) = await _cargoService.GetAllCargosAsync(search, status, page, pageSize);

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.TotalCount = totalCount;

            return View(items);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            return View(new CreateCargoDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCargoDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(dto);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                Guid? guidUserId = Guid.TryParse(userId, out var parsed) ? parsed : null;

                string trackCode = await _cargoService.CreateCargoAsync(dto, guidUserId);
                TempData["success"] = $"Kargo kabulü yapıldı. Takip Numarası: {trackCode}";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateDropdownsAsync();
                return View(dto);
            }
        }

        public async Task<IActionResult> Detail(Guid id)
        {
            var cargo = await _cargoService.GetCargoDetailByIdAsync(id);
            if (cargo == null)
            {
                TempData["error"] = "Kargo bulunamadı.";
                return RedirectToAction(nameof(Index));
            }
            return View(cargo);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(CargoStatusChangeDto dto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                Guid? guidUserId = Guid.TryParse(userId, out var parsed) ? parsed : null;

                await _cargoService.UpdateStatusAsync(dto, guidUserId, User.Identity?.Name);
                TempData["success"] = "Kargo durumu başarıyla güncellendi.";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Detail), new { id = dto.CargoId });
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                Guid? guidUserId = Guid.TryParse(userId, out var parsed) ? parsed : null;

                await _cargoService.DeleteAsync(id, guidUserId, User.Identity?.Name);
                TempData["success"] = "Kargo başarıyla silindi (Soft Delete).";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdownsAsync()
        {
            var branches = await _branchService.GetAllAsync();
            ViewBag.Branches = new SelectList(branches, "Id", "Name");

            var users = _userManager.Users.ToList();
            ViewBag.Users = new SelectList(users, "Id", "FullName");
        }
    }
}
