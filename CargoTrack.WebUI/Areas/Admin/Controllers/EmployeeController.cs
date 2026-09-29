using System;
using System.Threading.Tasks;
using CargoTrack.Business.Services.Branches;
using CargoTrack.Business.Services.Employees;
using CargoTrack.Business.Services.TransferCenters;
using CargoTrack.DTO.DTOs.EmployeeDtos;
using CargoTrack.WebUI.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CargoTrack.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class EmployeeController(
        IEmployeeService _employeeService,
        IBranchService _branchService,
        ITransferCenterService _transferCenterService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var list = await _employeeService.GetAllAsync();
            return View(list);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateEmployeeDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(dto);
            }

            await _employeeService.CreateAsync(dto);
            TempData["success"] = "Personel kaydı oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(Guid id)
        {
            var dto = await _employeeService.GetByIdAsync(id);
            await PopulateDropdownsAsync();
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateEmployeeDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(dto);
            }

            await _employeeService.UpdateAsync(dto);
            TempData["success"] = "Personel bilgileri güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            await _employeeService.DeleteAsync(id);
            TempData["success"] = "Personel silindi.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdownsAsync()
        {
            var branches = await _branchService.GetAllAsync();
            ViewBag.Branches = new SelectList(branches, "Id", "Name");

            var tcenters = await _transferCenterService.GetAllAsync();
            ViewBag.TransferCenters = new SelectList(tcenters, "Id", "Name");
        }
    }
}
