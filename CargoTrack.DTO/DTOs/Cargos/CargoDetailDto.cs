using System;
using System.Collections.Generic;
using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.DTO.DTOs.Cargos
{
    public class CargoDetailDto
    {
        public Guid Id { get; set; }
        public string TrackCode { get; set; } = string.Empty;
        public DateTime ShipmentDate { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public DateTime EstimatedDeliveryDate { get; set; }

        public double Weight { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Length { get; set; }
        public double Desi { get; set; }
        public decimal Price { get; set; }

        public CargoType CargoType { get; set; }
        public CargoStatus CargoStatus { get; set; }

        public Guid SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string SenderPhone { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;

        public Guid? ReceiverId { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverPhone { get; set; } = string.Empty;
        public string ReceiverAddress { get; set; } = string.Empty;

        public Guid OriginBranchId { get; set; }
        public string OriginBranchName { get; set; } = string.Empty;
        public string OriginCityName { get; set; } = string.Empty;

        public Guid DestinationBranchId { get; set; }
        public string DestinationBranchName { get; set; } = string.Empty;
        public string DestinationCityName { get; set; } = string.Empty;

        public string CurrentLocationName { get; set; } = string.Empty;
        public string? DeliveryPinCode { get; set; }
        public string? ReceivedBy { get; set; }
        public int FailedDeliveryAttempts { get; set; }
        public bool IsDelayed => CargoStatus != CargoStatus.Delivered && CargoStatus != CargoStatus.ReturnedToSender && CargoStatus != CargoStatus.Canceled && DateTime.Now > EstimatedDeliveryDate;

        public List<CargoMovementDto> Movements { get; set; } = new List<CargoMovementDto>();
        public List<DeliveryExceptionDetailDto> Exceptions { get; set; } = new List<DeliveryExceptionDetailDto>();
    }

    public class CargoMovementDto
    {
        public Guid Id { get; set; }
        public CargoStatus? PreviousStatus { get; set; }
        public CargoStatus NewStatus { get; set; }
        public DateTime Timestamp { get; set; }
        public string Description { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public string? EmployeeName { get; set; }
    }

    public class DeliveryExceptionDetailDto
    {
        public Guid Id { get; set; }
        public DateTime ExceptionDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public int AttemptNumber { get; set; }
        public string? EmployeeName { get; set; }
    }
}
