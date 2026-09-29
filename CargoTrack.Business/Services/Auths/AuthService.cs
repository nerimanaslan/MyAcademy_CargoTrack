using CargoTrack.DTO.DTOs.UserDtos;
using CargoTrack.Entity.Entities;

using Mapster;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace CargoTrack.Business.Services.Auths
{
    public class AuthService(UserManager<AppUser> _userManager) : IAuthService
    {
        public async Task<IdentityResult> RegisterUserAsync(RegisterUserDto registerUserDto)
        {
            // İş kuralı ve Mapster dönüşümü artık tam olması gerektiği yerde (Business katmanında)
            var user = registerUserDto.Adapt<AppUser>();

            var result = await _userManager.CreateAsync(user, registerUserDto.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
            }

            return result;
        }
    }
}
