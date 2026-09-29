using CargoTrack.DTO.DTOs.UserDtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargoTrack.Business.Services.Auths
{
    public interface IUserRoleService
    {
        Task<List<ResultUserDto>> GetUsersWithRolesAsync();
        Task<List<RoleAssignDto>> GetRoleAssignListAsync(Guid userId);
        Task<string> GetUserFullNameAsync(Guid userId);
        Task<bool> UpdateUserRolesAsync(Guid userId, List<RoleAssignDto> roleAssignDto);
    }
}
