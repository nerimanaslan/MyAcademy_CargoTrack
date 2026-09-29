using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CargoTrack.DataAccess.Repositories.Branches;
using CargoTrack.DataAccess.Repositories.CargoPrices;
using CargoTrack.DTO.DTOs.CargoPriceDtos;
using CargoTrack.Entity.Entities;
using CargoTrack.Entity.Entities.Enums;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace CargoTrack.Business.Services.CargoPrices
{
    public class CargoPricingService(
        ICargoPriceRepository _priceRepository,
        IBranchRepository _branchRepository) : ICargoPricingService
    {
        public async Task<PriceCalculationResultDto> CalculatePriceAsync(CalculatePriceDto dto)
        {
            var originBranch = await _branchRepository.GetByIdAsync(dto.OriginBranchId);
            if (originBranch == null)
                throw new ValidationException("Çıkış şubesi bulunamadı.");

            var destinationBranch = await _branchRepository.GetByIdAsync(dto.DestinationBranchId);
            if (destinationBranch == null)
                throw new ValidationException("Varış şubesi bulunamadı.");

            if (originBranch.Id == destinationBranch.Id)
                throw new ValidationException("Çıkış şubesi ile varış şubesi aynı olamaz.");

            double desi = Math.Round((dto.Width * dto.Height * dto.Length) / 3000.0, 2);
            double chargeableWeight = Math.Max(dto.Weight, desi);

            var policy = await _priceRepository.GetAsync(p => p.IsActive);
            if (policy == null)
            {
                policy = new CargoPrice();
            }

            bool isInterCity = originBranch.CityId != destinationBranch.CityId;
            int estimatedDays = isInterCity ? 2 : 1;

            decimal basePrice = policy.BasePrice;
            decimal weightFee = (decimal)chargeableWeight * Math.Max(policy.PerKgRate, policy.PerDesiRate);
            decimal subtotal = basePrice + weightFee;

            decimal distanceFee = 0;
            if (isInterCity)
            {
                distanceFee = subtotal * (policy.InterCityMultiplier - 1.00m);
                subtotal += distanceFee;
            }

            decimal extraServiceFee = 0;
            if (dto.CargoType == CargoType.Express)
            {
                extraServiceFee += subtotal * (policy.ExpressMultiplier - 1.00m);
                estimatedDays = Math.Max(1, estimatedDays - 1);
            }
            else if (dto.CargoType == CargoType.SameDay)
            {
                extraServiceFee += subtotal * (policy.SameDayMultiplier - 1.00m);
                estimatedDays = 0;
            }
            else if (dto.CargoType == CargoType.Fragile)
            {
                extraServiceFee += policy.FragileHandlingFee;
            }

            decimal totalPrice = Math.Round(subtotal + extraServiceFee, 2);

            return new PriceCalculationResultDto
            {
                Desi = desi,
                ChargeableWeight = chargeableWeight,
                BasePrice = basePrice,
                WeightFee = Math.Round(weightFee, 2),
                DistanceFee = Math.Round(distanceFee, 2),
                ExtraServiceFee = Math.Round(extraServiceFee, 2),
                TotalPrice = totalPrice,
                IsInterCity = isInterCity,
                EstimatedDays = estimatedDays
            };
        }

        public async Task<ResultCargoPriceDto> GetCurrentPricePolicyAsync()
        {
            var policy = await _priceRepository.GetAsync(p => p.IsActive);
            if (policy == null)
            {
                policy = new CargoPrice();
                await _priceRepository.CreateAsync(policy);
            }
            return policy.Adapt<ResultCargoPriceDto>();
        }

        public async Task UpdatePricePolicyAsync(UpdateCargoPriceDto dto)
        {
            var policy = await _priceRepository.GetByIdAsync(dto.Id);
            if (policy != null)
            {
                dto.Adapt(policy);
                await _priceRepository.UpdateAsync(policy);
            }
        }
    }
}
