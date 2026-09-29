using System;
using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.DTO.DTOs.CargoPriceDtos
{
    public class ResultCargoPriceDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public decimal PerKgRate { get; set; }
        public decimal PerDesiRate { get; set; }
        public decimal InterCityMultiplier { get; set; }
        public decimal ExpressMultiplier { get; set; }
        public decimal SameDayMultiplier { get; set; }
        public decimal FragileHandlingFee { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateCargoPriceDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public decimal PerKgRate { get; set; }
        public decimal PerDesiRate { get; set; }
        public decimal InterCityMultiplier { get; set; }
        public decimal ExpressMultiplier { get; set; }
        public decimal SameDayMultiplier { get; set; }
        public decimal FragileHandlingFee { get; set; }
        public bool IsActive { get; set; }
    }

    public class CalculatePriceDto
    {
        public double Weight { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Length { get; set; }
        public Guid OriginBranchId { get; set; }
        public Guid DestinationBranchId { get; set; }
        public CargoType CargoType { get; set; } = CargoType.Standard;
    }

    public class PriceCalculationResultDto
    {
        public double Desi { get; set; }
        public double ChargeableWeight { get; set; }
        public decimal BasePrice { get; set; }
        public decimal WeightFee { get; set; }
        public decimal DistanceFee { get; set; }
        public decimal ExtraServiceFee { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsInterCity { get; set; }
        public int EstimatedDays { get; set; }
    }
}
