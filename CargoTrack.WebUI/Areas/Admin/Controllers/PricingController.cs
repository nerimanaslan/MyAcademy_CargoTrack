using System.Threading.Tasks;
using CargoTrack.Business.Services.CargoPrices;
using CargoTrack.DTO.DTOs.CargoPriceDtos;
using CargoTrack.WebUI.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CargoTrack.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class PricingController(ICargoPricingService _pricingService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var policy = await _pricingService.GetCurrentPricePolicyAsync();
            return View(policy);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateCargoPriceDto dto)
        {
            await _pricingService.UpdatePricePolicyAsync(dto);
            TempData["success"] = "Fiyatlandırma politikası ve katsayılar güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Calculate([FromBody] CalculatePriceDto dto)
        {
            var result = await _pricingService.CalculatePriceAsync(dto);
            return Json(result);
        }
    }
}
