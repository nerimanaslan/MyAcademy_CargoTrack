using System.Threading.Tasks;
using CargoTrack.DTO.DTOs.CargoPriceDtos;
using CargoTrack.Entity.Entities;

namespace CargoTrack.Business.Services.CargoPrices
{
    public interface ICargoPricingService
    {
        Task<PriceCalculationResultDto> CalculatePriceAsync(CalculatePriceDto dto);
        Task<ResultCargoPriceDto> GetCurrentPricePolicyAsync();
        Task UpdatePricePolicyAsync(UpdateCargoPriceDto dto);
    }
}
