using System;
using CargoTrack.Entity.Entities.Common;
using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.Entity.Entities
{
    public class CargoMovement : BaseEntity
    {
        public Guid CargoId { get; set; }
        public virtual Cargo Cargo { get; set; }

        public CargoStatus? PreviousStatus { get; set; }
        public CargoStatus NewStatus { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Description { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;

        public Guid? BranchId { get; set; }
        public virtual Branch? Branch { get; set; }

        public Guid? TransferCenterId { get; set; }
        public virtual TransferCenter? TransferCenter { get; set; }

        public Guid? EmployeeId { get; set; }
        public virtual Employee? Employee { get; set; }
    }
}
