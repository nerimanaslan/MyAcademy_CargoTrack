using CargoTrack.DTO.DTOs.BranchDtos;

namespace CargoTrack.Business.Services.Branches
{
    public interface IBranchService
    {
        Task CreateAsync(CreateBranchDto createBranchDto);
        Task DeleteAsync(Guid id);
        Task<List<ResultBranchDto>> GetAllAsync();
        Task<UpdateBranchDto> GetByIdAsync(Guid id);
        Task UpdateAsync(UpdateBranchDto updateBranchDto);
    }
}
