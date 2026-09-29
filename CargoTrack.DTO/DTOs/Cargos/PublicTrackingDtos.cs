using System;
using System.Collections.Generic;
using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.DTO.DTOs.Cargos
{
    public class PublicCargoTrackingDto
    {
        public string TrackCode { get; set; } = string.Empty;
        public CargoStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string StatusBadgeClass { get; set; } = string.Empty;
        public string StatusIcon { get; set; } = "local_shipping";

        public string ShipmentDateFormatted { get; set; } = string.Empty;
        public string EstimatedDeliveryFormatted { get; set; } = string.Empty;
        public string EstimatedDeliveryTitle { get; set; } = string.Empty;
        public string EstimatedTimeWindow { get; set; } = "09:00 - 18:00";
        public int ProgressPercentage { get; set; } = 20;

        public string OriginCity { get; set; } = string.Empty;
        public string OriginDistrict { get; set; } = string.Empty;
        public string DestinationCity { get; set; } = string.Empty;
        public string DestinationDistrict { get; set; } = string.Empty;

        public string MaskedSenderName { get; set; } = string.Empty;
        public string SenderLocation { get; set; } = string.Empty;

        public string MaskedReceiverName { get; set; } = string.Empty;
        public string ReceiverLocation { get; set; } = string.Empty;

        public string WeightFormatted { get; set; } = string.Empty;
        public string CargoTypeDescription { get; set; } = string.Empty;
        public string OriginBranchName { get; set; } = string.Empty;
        public string DestinationBranchName { get; set; } = string.Empty;

        public List<PublicMovementItemDto> Movements { get; set; } = new List<PublicMovementItemDto>();
    }

    public class PublicMovementItemDto
    {
        public string Title { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string DateFormatted { get; set; } = string.Empty;
        public string TimeFormatted { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public bool IsCurrent { get; set; }
        public string IconName { get; set; } = "check";
        public string CircleColorClass { get; set; } = "bg-green-500 text-white";
    }

    public class PublicRecentShipmentDto
    {
        public string TrackCode { get; set; } = string.Empty;
        public string StatusText { get; set; } = string.Empty;
        public string StatusBadgeClass { get; set; } = string.Empty;
        public string StatusIcon { get; set; } = "local_shipping";
        public string OriginCity { get; set; } = string.Empty;
        public string DestinationCity { get; set; } = string.Empty;
        public string DateLabel { get; set; } = "Tahmini Teslimat";
        public string DateFormatted { get; set; } = string.Empty;
        public string BorderColorClass { get; set; } = "border-l-surface-tint";
    }
}
