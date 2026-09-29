using System;
using System.Linq;
using System.Threading.Tasks;
using CargoTrack.Business.Services.Branches;
using CargoTrack.Business.Services.Dashboards;
using CargoTrack.Entity.Entities;
using CargoTrack.WebUI.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CargoTrack.WebUI.Areas.Manager.Controllers
{
    [Area("Manager")]
    [Authorize(Roles = Roles.Manager)]
    public class DashboardController(
        IDashboardService _dashboardService,
        IBranchService _branchService,
        UserManager<AppUser> _userManager) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            Guid branchId;

            if (user?.BranchId.HasValue == true)
            {
                branchId = user.BranchId.Value;
            }
            else
            {
                return Forbid();
            }

            var model = await _dashboardService.GetManagerDashboardAsync(branchId);
            return View(model);
        }
    }
}
