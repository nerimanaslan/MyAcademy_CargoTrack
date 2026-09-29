using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CargoTrack.DTO.DTOs.TransferCenterDtos;

namespace CargoTrack.Business.Services.TransferCenters
{
    public interface ITransferCenterService
    {
        Task CreateAsync(CreateTransferCenterDto dto);
        Task UpdateAsync(UpdateTransferCenterDto dto);
        Task DeleteAsync(Guid id);
        Task<List<ResultTransferCenterDto>> GetAllAsync();
        Task<UpdateTransferCenterDto> GetByIdAsync(Guid id);
    }
}
