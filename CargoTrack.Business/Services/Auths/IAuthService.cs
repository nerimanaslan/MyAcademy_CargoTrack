using CargoTrack.DTO.DTOs.UserDtos;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace CargoTrack.Business.Services.Auths
{
    public interface IAuthService
    {
        Task<IdentityResult> RegisterUserAsync(RegisterUserDto registerUserDto);
    }
}
