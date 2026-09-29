using CargoTrack.Business.Services.Cargos;
using CargoTrack.Entity.Entities;
using CargoTrack.Entity.Entities.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CargoTrack.WebUI.ViewComponents.ManagerDashboard
{
    public class ManagerStatsViewComponent(
        ICargoService _cargoService,
        UserManager<AppUser> _userManager) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var branchId = user?.BranchId ?? System.Guid.Empty;

            var (cargos, _) = await _cargoService.GetCargosByBranchAsync(branchId, null, null, 1, 1000);

            ViewBag.TotalCargos = cargos.Count;
            ViewBag.OutForDelivery = cargos.Count(c => c.CargoStatus == CargoStatus.OutForDelivery);
            ViewBag.Delivered = cargos.Count(c => c.CargoStatus == CargoStatus.Delivered);
            ViewBag.Problems = cargos.Count(c => c.CargoStatus == CargoStatus.DeliveryFailed || c.CargoStatus == CargoStatus.ReturnProcess);

            return View();
        }
    }
}