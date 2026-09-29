using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CargoTrack.DataAccess.Repositories.TransferCenters;
using CargoTrack.DTO.DTOs.TransferCenterDtos;
using CargoTrack.Entity.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace CargoTrack.Business.Services.TransferCenters
{
    public class TransferCenterService(ITransferCenterRepository _transferCenterRepository) : ITransferCenterService
    {
        public async Task CreateAsync(CreateTransferCenterDto dto)
        {
            var entity = dto.Adapt<TransferCenter>();
            await _transferCenterRepository.CreateAsync(entity);
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _transferCenterRepository.GetByIdAsync(id);
            if (entity is null)
                throw new ValidationException("Transfer Merkezi bulunamadı.");

            await _transferCenterRepository.DeleteAsync(entity);
        }

        public async Task<List<ResultTransferCenterDto>> GetAllAsync()
        {
            var list = await _transferCenterRepository.GetListAsync(include: q => q.Include(x => x.City));
            return list.Adapt<List<ResultTransferCenterDto>>();
        }

        public async Task<UpdateTransferCenterDto> GetByIdAsync(Guid id)
        {
            var entity = await _transferCenterRepository.GetByIdAsync(id);
            if (entity is null)
                throw new ValidationException("Transfer Merkezi bulunamadı.");

            return entity.Adapt<UpdateTransferCenterDto>();
        }

        public async Task UpdateAsync(UpdateTransferCenterDto dto)
        {
            var entity = await _transferCenterRepository.GetByIdAsync(dto.Id);
            if (entity is null)
                throw new ValidationException("Transfer Merkezi bulunamadı.");

            dto.Adapt(entity);
            await _transferCenterRepository.UpdateAsync(entity);
        }
    }
}
