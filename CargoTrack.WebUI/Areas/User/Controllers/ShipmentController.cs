using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CargoTrack.Business.Services.Branches;
using CargoTrack.Business.Services.CargoPrices;
using CargoTrack.Business.Services.Cargos;
using CargoTrack.DTO.DTOs.CargoPriceDtos;
using CargoTrack.DTO.DTOs.Cargos;
using CargoTrack.Entity.Entities;
using CargoTrack.WebUI.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CargoTrack.WebUI.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = Roles.User)]
    public class ShipmentController(
        ICargoService _cargoService,
        IBranchService _branchService,
        ICargoPricingService _pricingService,
        UserManager<AppUser> _userManager) : Controller
    {
        public async Task<IActionResult> SendCargo()
        {
            var branches = await _branchService.GetAllAsync();
            ViewBag.Branches = new SelectList(branches, "Id", "Name");
            return View(new CreateCargoDto());
        }

        [HttpPost]
        public async Task<IActionResult> SendCargo(CreateCargoDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index", "Login", new { area = "" });
            }

            dto.SenderId = user.Id;
            dto.CustomPrice = null;

            if (!ModelState.IsValid)
            {
                var branches = await _branchService.GetAllAsync();
                ViewBag.Branches = new SelectList(branches, "Id", "Name");
                return View(dto);
            }

            try
            {
                string trackCode = await _cargoService.CreateCargoAsync(dto, user.Id);
                TempData["success"] = $"Kargo gönderi talebiniz alındı! Takip Numaranız: {trackCode}";
                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var branches = await _branchService.GetAllAsync();
                ViewBag.Branches = new SelectList(branches, "Id", "Name");
                return View(dto);
            }
        }

        public async Task<IActionResult> CalculatePrice()
        {
            var branches = await _branchService.GetAllAsync();
            ViewBag.Branches = new SelectList(branches, "Id", "Name");
            return View(new CalculatePriceDto
            {
                Weight = 2,
                Width = 20,
                Length = 25,
                Height = 15
            });
        }

        [HttpPost]
        public async Task<IActionResult> CalculatePrice(CalculatePriceDto dto)
        {
            var branches = await _branchService.GetAllAsync();
            ViewBag.Branches = new SelectList(branches, "Id", "Name");

            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var result = await _pricingService.CalculatePriceAsync(dto);
            ViewBag.Result = result;
            return View(dto);
        }
    }
}
