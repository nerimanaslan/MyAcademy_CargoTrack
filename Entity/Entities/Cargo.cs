using System;
using System.Collections.Generic;
using CargoTrack.Entity.Entities.Common;
using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.Entity.Entities
{
    public class Cargo : BaseEntity
    {
        public string TrackCode { get; set; } = string.Empty;
        public DateTime ShipmentDate { get; set; } = DateTime.Now;
        public DateTime? ArrivalDate { get; set; }
        public DateTime EstimatedDeliveryDate { get; set; } = DateTime.Now.AddDays(2);

        public double Weight { get; set; } // kg
        public double Width { get; set; } = 10;  // cm
        public double Height { get; set; } = 10; // cm
        public double Length { get; set; } = 10; // cm
        public double Desi { get; set; }   // (Width * Height * Length) / 3000
        public decimal Price { get; set; }

        public CargoType CargoType { get; set; } = CargoType.Standard;
        public CargoStatus CargoStatus { get; set; } = CargoStatus.Created;

        public Guid SenderId { get; set; }
        public virtual AppUser Sender { get; set; }

        public Guid? ReceiverId { get; set; }
        public virtual AppUser? Receiver { get; set; }

        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverPhone { get; set; } = string.Empty;
        public string ReceiverAddress { get; set; } = string.Empty;

        public Guid OriginBranchId { get; set; }
        public virtual Branch OriginBranch { get; set; }

        public Guid DestinationBranchId { get; set; }
        public virtual Branch DestinationBranch { get; set; }

        public Guid? CurrentBranchId { get; set; }
        public virtual Branch? CurrentBranch { get; set; }

        public Guid? CurrentTransferCenterId { get; set; }
        public virtual TransferCenter? CurrentTransferCenter { get; set; }

        // Delivery verification & SLA
        public string? DeliveryPinCode { get; set; } // 6-digit delivery security PIN
        public string? ReceivedBy { get; set; }
        public Guid? DeliveredByEmployeeId { get; set; }
        public virtual Employee? DeliveredByEmployee { get; set; }

        public int FailedDeliveryAttempts { get; set; } = 0; // After 3 attempts -> ReturnProcess

        // Navigation Collections
        public virtual IList<CargoMovement> Movements { get; set; } = new List<CargoMovement>();
        public virtual IList<DeliveryException> Exceptions { get; set; } = new List<DeliveryException>();
        public virtual IList<Delivery> Deliveries { get; set; } = new List<Delivery>();
    }
}
