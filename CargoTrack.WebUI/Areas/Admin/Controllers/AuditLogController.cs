using System.Threading.Tasks;
using CargoTrack.Business.Services.AuditLogs;
using CargoTrack.WebUI.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CargoTrack.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class AuditLogController(IAuditLogService _auditLogService) : Controller
    {
        public async Task<IActionResult> Index(string? entityName, string? search)
        {
            var logs = await _auditLogService.GetAllLogsAsync(entityName, search);
            ViewBag.CurrentEntity = entityName;
            ViewBag.CurrentSearch = search;
            return View(logs);
        }
    }
}
