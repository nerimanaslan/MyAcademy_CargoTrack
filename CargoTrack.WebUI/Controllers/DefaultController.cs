using System.Threading.Tasks;
using CargoTrack.Business.Services.Cargos;
using Microsoft.AspNetCore.Mvc;

namespace CargoTrack.WebUI.Controllers
{
    public class DefaultController(ICargoService _cargoService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            if (TempData["error"] != null)
            {
                ViewBag.error = TempData["error"];
            }

            var recentShipments = await _cargoService.GetRecentPublicShipmentsAsync(6);
            return View(recentShipments);
        }

        public async Task<IActionResult> CargoDetails(string trackCode)
        {
            if (string.IsNullOrWhiteSpace(trackCode))
            {
                TempData["error"] = "Lütfen sorgulamak istediğiniz kargo takip numarasını giriniz.";
                return RedirectToAction(nameof(Index));
            }

            var cargo = await _cargoService.GetPublicTrackingByCodeAsync(trackCode.Trim());

            if (cargo is null)
            {
                TempData["error"] = $"'{trackCode}' takip numarasına ait bir kargo bulunamadı. Lütfen numarayı kontrol ediniz.";
                return RedirectToAction(nameof(Index));
            }

            return View(cargo);
        }
    }
}
