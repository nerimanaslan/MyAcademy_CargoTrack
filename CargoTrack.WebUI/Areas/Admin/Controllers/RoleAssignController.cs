using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CargoTrack.Business.Services.Auths;
using CargoTrack.DTO.DTOs.UserDtos;
using CargoTrack.WebUI.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CargoTrack.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class RoleAssignController(IUserRoleService _userRoleService) : Controller
    {
        public async Task<IActionResult> Index()
        {
           
            var usersWithRoles = await _userRoleService.GetUsersWithRolesAsync();
            return View(usersWithRoles);
        }

        [HttpGet]
        public async Task<IActionResult> AssignRole(Guid id)
        {
            
            var roleAssignList = await _userRoleService.GetRoleAssignListAsync(id);
            if (roleAssignList.Count == 0) return RedirectToAction("Index");

            ViewBag.fullName = await _userRoleService.GetUserFullNameAsync(id);
            TempData["UserId"] = id;
            return View(roleAssignList);
        }

        [HttpPost]
        public async Task<IActionResult> AssignRole(List<RoleAssignDto> roleAssignDto)
        {
            if (TempData["UserId"] == null) return RedirectToAction("Index");

            var userId = Guid.Parse(TempData["UserId"].ToString());

            
            var success = await _userRoleService.UpdateUserRolesAsync(userId, roleAssignDto);

            if (success)
            {
                TempData["success"] = "Kullanıcı rolleri başarıyla güncellendi.";
            }

            return RedirectToAction("Index");
        }
    }
}