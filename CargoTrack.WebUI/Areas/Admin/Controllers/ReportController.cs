using System.Threading.Tasks;
using CargoTrack.Business.Services.Dashboards;
using CargoTrack.WebUI.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CargoTrack.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class ReportController(IDashboardService _dashboardService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var reports = await _dashboardService.GetPerformanceReportsAsync();
            return View(reports);
        }
    }
}
