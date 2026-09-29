using CargoTrack.Business.Services.Auths;
using CargoTrack.DTO.DTOs.UserDtos;
using CargoTrack.Entity.Entities;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CargoTrack.Business.Services.Auths
{
    public class UserRoleService(UserManager<AppUser> _userManager, RoleManager<AppRole> _roleManager) : IUserRoleService
    {
        public async Task<List<ResultUserDto>> GetUsersWithRolesAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            // İş kuralı ve Mapster dönüşümü (Hocanın kuralı gereği) burada yapılıyor!
            var mappedUsers = users.Adapt<List<ResultUserDto>>();

            foreach (var user in mappedUsers)
            {
                var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
                if (appUser != null)
                {
                    user.Roles = await _userManager.GetRolesAsync(appUser);
                }
            }

            return mappedUsers;
        }

        public async Task<List<RoleAssignDto>> GetRoleAssignListAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return new List<RoleAssignDto>();

            var roles = await _roleManager.Roles.ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);

            var roleAssignList = new List<RoleAssignDto>();

            foreach (var role in roles)
            {
                var roleAssignDto = new RoleAssignDto
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    RoleExist = userRoles.Contains(role.Name)
                };
                roleAssignList.Add(roleAssignDto);
            }

            return roleAssignList;
        }

        public async Task<string> GetUserFullNameAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            return user != null ? $"{user.FirstName} {user.LastName}" : string.Empty;
        }

        public async Task<bool> UpdateUserRolesAsync(Guid userId, List<RoleAssignDto> roleAssignDto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            foreach (var item in roleAssignDto)
            {
                if (item.RoleExist)
                {
                    if (!await _userManager.IsInRoleAsync(user, item.RoleName))
                    {
                        await _userManager.AddToRoleAsync(user, item.RoleName);
                    }
                }
                else
                {
                    if (await _userManager.IsInRoleAsync(user, item.RoleName))
                    {
                        await _userManager.RemoveFromRoleAsync(user, item.RoleName);
                    }
                }
            }
            return true;
        }
    }
}
