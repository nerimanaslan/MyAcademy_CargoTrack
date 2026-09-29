using CargoTrack.Entity.Entities.Common;

namespace CargoTrack.Entity.Entities
{
    public class CargoPrice : BaseEntity
    {
        public string Name { get; set; } = "Standart Fiyatlandırma Politikası";
        public decimal BasePrice { get; set; } = 45.00m;
        public decimal PerKgRate { get; set; } = 12.00m;
        public decimal PerDesiRate { get; set; } = 10.00m;
        public decimal InterCityMultiplier { get; set; } = 1.35m;
        public decimal ExpressMultiplier { get; set; } = 1.50m;
        public decimal SameDayMultiplier { get; set; } = 2.00m;
        public decimal FragileHandlingFee { get; set; } = 25.00m;
        public bool IsActive { get; set; } = true;
    }
}
