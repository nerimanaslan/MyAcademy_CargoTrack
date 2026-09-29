using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CargoTrack.DataAccess.Repositories.Employees;
using CargoTrack.DTO.DTOs.EmployeeDtos;
using CargoTrack.Entity.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace CargoTrack.Business.Services.Employees
{
    public class EmployeeService(IEmployeeRepository _employeeRepository) : IEmployeeService
    {
        public async Task CreateAsync(CreateEmployeeDto dto)
        {
            var entity = dto.Adapt<Employee>();
            await _employeeRepository.CreateAsync(entity);
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _employeeRepository.GetByIdAsync(id);
            if (entity is null)
                throw new ValidationException("Personel bulunamadı.");

            await _employeeRepository.DeleteAsync(entity);
        }

        public async Task<List<ResultEmployeeDto>> GetAllAsync()
        {
            var list = await _employeeRepository.GetListAsync(
                include: q => q.Include(x => x.Branch).Include(x => x.TransferCenter).Include(x => x.Deliveries)
            );

            var result = new List<ResultEmployeeDto>();
            foreach (var emp in list)
            {
                result.Add(new ResultEmployeeDto
                {
                    Id = emp.Id,
                    FirstName = emp.FirstName,
                    LastName = emp.LastName,
                    Title = emp.Title,
                    Phone = emp.Phone,
                    Email = emp.Email,
                    BranchId = emp.BranchId,
                    BranchName = emp.Branch?.Name,
                    TransferCenterId = emp.TransferCenterId,
                    TransferCenterName = emp.TransferCenter?.Name,
                    IsActive = emp.IsActive,
                    TotalDeliveries = emp.Deliveries?.Count ?? 0
                });
            }
            return result;
        }

        public async Task<List<ResultEmployeeDto>> GetByBranchIdAsync(Guid branchId)
        {
            var list = await _employeeRepository.GetListAsync(
                predicate: x => x.BranchId == branchId && x.IsActive,
                include: q => q.Include(x => x.Branch).Include(x => x.Deliveries)
            );

            var result = new List<ResultEmployeeDto>();
            foreach (var emp in list)
            {
                result.Add(new ResultEmployeeDto
                {
                    Id = emp.Id,
                    FirstName = emp.FirstName,
                    LastName = emp.LastName,
                    Title = emp.Title,
                    Phone = emp.Phone,
                    Email = emp.Email,
                    BranchId = emp.BranchId,
                    BranchName = emp.Branch?.Name,
                    IsActive = emp.IsActive,
                    TotalDeliveries = emp.Deliveries?.Count ?? 0
                });
            }
            return result;
        }

        public async Task<UpdateEmployeeDto> GetByIdAsync(Guid id)
        {
            var entity = await _employeeRepository.GetByIdAsync(id);
            if (entity is null)
                throw new ValidationException("Personel bulunamadı.");

            return entity.Adapt<UpdateEmployeeDto>();
        }

        public async Task UpdateAsync(UpdateEmployeeDto dto)
        {
            var entity = await _employeeRepository.GetByIdAsync(dto.Id);
            if (entity is null)
                throw new ValidationException("Personel bulunamadı.");

            dto.Adapt(entity);
            await _employeeRepository.UpdateAsync(entity);
        }
    }
}
