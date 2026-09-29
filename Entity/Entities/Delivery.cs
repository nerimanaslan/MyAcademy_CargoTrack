using System;
using CargoTrack.Entity.Entities.Common;

namespace CargoTrack.Entity.Entities
{
    public class Delivery : BaseEntity
    {
        public Guid CargoId { get; set; }
        public virtual Cargo Cargo { get; set; }

        public DateTime DeliveryDate { get; set; } = DateTime.Now;
        public string ReceivedBy { get; set; } = string.Empty; // Teslim Alan Kişi
        public string? ReceiverRelationship { get; set; } // Kendisi, Aile Bireyi, İş Arkadaşı, Güvenlik
        public bool DeliveryPinCodeVerified { get; set; } = true;

        public Guid? DeliveredByEmployeeId { get; set; }
        public virtual Employee? DeliveredByEmployee { get; set; }

        public string? Notes { get; set; }
    }
}
