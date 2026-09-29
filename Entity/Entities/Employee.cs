using System;
using System.Collections.Generic;
using CargoTrack.Entity.Entities.Common;

namespace CargoTrack.Entity.Entities
{
    public class Employee : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty; // Kurye, Şube Müdürü, Operasyon Personeli, Transfer Merkezi Yetkilisi
        public string? Phone { get; set; }
        public string? Email { get; set; }

        public Guid? BranchId { get; set; }
        public virtual Branch? Branch { get; set; }

        public Guid? TransferCenterId { get; set; }
        public virtual TransferCenter? TransferCenter { get; set; }

        public Guid? UserId { get; set; }
        public virtual AppUser? User { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual IList<CargoMovement> Movements { get; set; } = new List<CargoMovement>();
        public virtual IList<Delivery> Deliveries { get; set; } = new List<Delivery>();
        public virtual IList<DeliveryException> Exceptions { get; set; } = new List<DeliveryException>();

        public string FullName => $"{FirstName} {LastName}";
    }
}
