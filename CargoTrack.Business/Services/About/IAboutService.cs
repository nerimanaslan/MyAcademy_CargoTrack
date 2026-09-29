using CargoTrack.DTO.DTOs.AboutDtos;

namespace CargoTrack.Business.Services.Abouts
{
    public interface IAboutService
    {
        Task CreateAsync(CreateAboutDto createAboutDto);
        Task DeleteAsync(Guid id);
        Task<List<ResultAboutDto>> GetAllAsync();
        Task<UpdateAboutDto> GetByIdAsync(Guid id);
        Task UpdateAsync(UpdateAboutDto updateAboutDto);
    }
}
