using System;
using System.Collections.Generic;
using CargoTrack.Entity.Entities.Common;

namespace CargoTrack.Entity.Entities
{
    public class Branch : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public Guid CityId { get; set; }
        public string? AddressDetail { get; set; }
        public string? Phone { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual City City { get; set; }
        public virtual IList<Cargo> OriginCargos { get; set; } = new List<Cargo>();
        public virtual IList<Cargo> DestinationCargos { get; set; } = new List<Cargo>();
        public virtual IList<Employee> Employees { get; set; } = new List<Employee>();
        public virtual IList<AppUser> Users { get; set; } = new List<AppUser>();
        public virtual IList<CargoMovement> Movements { get; set; } = new List<CargoMovement>();
    }
}
