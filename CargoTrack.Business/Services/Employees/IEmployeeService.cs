using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CargoTrack.DTO.DTOs.EmployeeDtos;

namespace CargoTrack.Business.Services.Employees
{
    public interface IEmployeeService
    {
        Task CreateAsync(CreateEmployeeDto dto);
        Task UpdateAsync(UpdateEmployeeDto dto);
        Task DeleteAsync(Guid id);
        Task<List<ResultEmployeeDto>> GetAllAsync();
        Task<UpdateEmployeeDto> GetByIdAsync(Guid id);
        Task<List<ResultEmployeeDto>> GetByBranchIdAsync(Guid branchId);
    }
}
