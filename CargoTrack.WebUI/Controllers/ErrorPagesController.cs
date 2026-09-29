using Microsoft.AspNetCore.Mvc;

namespace CargoTrack.WebUI.Controllers
{
    public class ErrorPagesController : Controller
    {
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
