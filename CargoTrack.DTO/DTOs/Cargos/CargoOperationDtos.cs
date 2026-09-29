using System;
using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.DTO.DTOs.Cargos
{
    public class CargoStatusChangeDto
    {
        public Guid CargoId { get; set; }
        public CargoStatus NewStatus { get; set; }
        public string Description { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public Guid? BranchId { get; set; }
        public Guid? TransferCenterId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string? DeliveryPinCode { get; set; }
        public string? ReceivedBy { get; set; }
    }

    public class VerifyDeliveryCodeDto
    {
        public Guid CargoId { get; set; }
        public string DeliveryCode { get; set; } = string.Empty;
        public string ReceivedBy { get; set; } = string.Empty;
        public string? ReceiverRelationship { get; set; }
        public string? Notes { get; set; }
        public Guid? EmployeeId { get; set; }
    }

    public class CreateDeliveryExceptionDto
    {
        public Guid CargoId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public Guid? EmployeeId { get; set; }
    }
}
