using System;
using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.DTO.DTOs.Cargos
{
    public class CreateCargoDto
    {
        public Guid SenderId { get; set; }
        public Guid? ReceiverId { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverPhone { get; set; } = string.Empty;
        public string ReceiverAddress { get; set; } = string.Empty;

        public Guid OriginBranchId { get; set; }
        public Guid DestinationBranchId { get; set; }

        public double Weight { get; set; }
        public double Width { get; set; } = 10;
        public double Height { get; set; } = 10;
        public double Length { get; set; } = 10;

        public CargoType CargoType { get; set; } = CargoType.Standard;
        public decimal? CustomPrice { get; set; }
    }
}
