using System;
using CargoTrack.Entity.Entities.Common;

namespace CargoTrack.Entity.Entities
{
    public class Customer : BaseEntity
    {
        public Guid UserId { get; set; }
        public virtual AppUser User { get; set; }

        public string CustomerCode { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? CompanyName { get; set; }
        public string? TaxNumber { get; set; }
        public string? DefaultAddress { get; set; }
    }
}
