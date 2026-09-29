using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CargoTrack.Business.Services.Dashboards;
using CargoTrack.Entity.Entities;
using CargoTrack.WebUI.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CargoTrack.WebUI.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = Roles.User)]
    public class DashboardController(
        IDashboardService _dashboardService,
        UserManager<AppUser> _userManager) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index", "Login", new { area = "" });
            }

            var model = await _dashboardService.GetUserDashboardAsync(user.Id);
            return View(model);
        }
    }
}
