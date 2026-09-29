using System;
using System.Collections.Generic;
using CargoTrack.Entity.Entities.Common;

namespace CargoTrack.Entity.Entities
{
    public class TransferCenter : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public Guid CityId { get; set; }
        public virtual City City { get; set; }
        public string? AddressDetail { get; set; }
        public int DailyCapacity { get; set; } = 5000;
        public bool IsActive { get; set; } = true;

        public virtual IList<Employee> Employees { get; set; } = new List<Employee>();
        public virtual IList<CargoMovement> Movements { get; set; } = new List<CargoMovement>();
    }
}
