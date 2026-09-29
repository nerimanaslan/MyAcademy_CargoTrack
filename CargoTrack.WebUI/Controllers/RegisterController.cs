using CargoTrack.Business.Services.Auths;
using CargoTrack.DTO.DTOs.UserDtos;
using Microsoft.AspNetCore.Mvc;

namespace CargoTrack.WebUI.Controllers
{
    public class RegisterController(IAuthService _authService) : Controller
    {
        [HttpGet]
        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Signup(RegisterUserDto registerUserDto)
        {
            if (!ModelState.IsValid)
            {
                return View(registerUserDto);
            }

            // Ýþ kuralý ve Mapster dönüþümü tamamen AuthService (Business) içerisine gizlendi!
            var result = await _authService.RegisterUserAsync(registerUserDto);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return View(registerUserDto);
            }

            return RedirectToAction("Index", "Login");
        }
    }
}