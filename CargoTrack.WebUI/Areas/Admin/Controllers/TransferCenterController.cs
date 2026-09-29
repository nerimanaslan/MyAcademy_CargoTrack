using System;
using System.Threading.Tasks;
using CargoTrack.Business.Services.Cities;
using CargoTrack.Business.Services.TransferCenters;
using CargoTrack.DTO.DTOs.TransferCenterDtos;
using CargoTrack.WebUI.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CargoTrack.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class TransferCenterController(
        ITransferCenterService _transferCenterService,
        ICityService _cityService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var list = await _transferCenterService.GetAllAsync();
            return View(list);
        }

        public async Task<IActionResult> Create()
        {
            var cities = await _cityService.GetAllAsync();
            ViewBag.Cities = new SelectList(cities, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTransferCenterDto dto)
        {
            if (!ModelState.IsValid)
            {
                var cities = await _cityService.GetAllAsync();
                ViewBag.Cities = new SelectList(cities, "Id", "Name");
                return View(dto);
            }

            await _transferCenterService.CreateAsync(dto);
            TempData["success"] = "Transfer merkezi başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(Guid id)
        {
            var dto = await _transferCenterService.GetByIdAsync(id);
            var cities = await _cityService.GetAllAsync();
            ViewBag.Cities = new SelectList(cities, "Id", "Name");
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateTransferCenterDto dto)
        {
            if (!ModelState.IsValid)
            {
                var cities = await _cityService.GetAllAsync();
                ViewBag.Cities = new SelectList(cities, "Id", "Name");
                return View(dto);
            }

            await _transferCenterService.UpdateAsync(dto);
            TempData["success"] = "Transfer merkezi güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            await _transferCenterService.DeleteAsync(id);
            TempData["success"] = "Transfer merkezi silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
