using System;
using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.DTO.DTOs.Cargos
{
    public class ResultCargoDto
    {
        public Guid Id { get; set; }
        public string TrackCode { get; set; } = string.Empty;
        public DateTime ShipmentDate { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public DateTime EstimatedDeliveryDate { get; set; }
        public double Weight { get; set; }
        public double Desi { get; set; }
        public decimal Price { get; set; }
        public CargoType CargoType { get; set; }
        public CargoStatus CargoStatus { get; set; }

        public Guid SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public Guid? ReceiverId { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverPhone { get; set; } = string.Empty;
        public string ReceiverAddress { get; set; } = string.Empty;

        public Guid OriginBranchId { get; set; }
        public string OriginBranchName { get; set; } = string.Empty;
        public Guid DestinationBranchId { get; set; }
        public string DestinationBranchName { get; set; } = string.Empty;

        public string CurrentLocationName { get; set; } = string.Empty;
        public string? DeliveryPinCode { get; set; }
        public string? ReceivedBy { get; set; }
        public int FailedDeliveryAttempts { get; set; }
        public bool IsDelayed => CargoStatus != CargoStatus.Delivered && CargoStatus != CargoStatus.ReturnedToSender && CargoStatus != CargoStatus.Canceled && DateTime.Now > EstimatedDeliveryDate;
    }
}
