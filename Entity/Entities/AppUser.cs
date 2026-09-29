using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace CargoTrack.Entity.Entities
{
    public class AppUser : IdentityUser<Guid>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public Guid? BranchId { get; set; }
        public virtual Branch? Branch { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();

        // Navigation Properties
        public virtual IList<Cargo> SentCargos { get; set; } = new List<Cargo>();
        public virtual IList<Cargo> ReceivedCargos { get; set; } = new List<Cargo>();
        public virtual IList<Address> Addresses { get; set; } = new List<Address>();
        public virtual IList<Customer> Customers { get; set; } = new List<Customer>();
        public virtual IList<Employee> Employees { get; set; } = new List<Employee>();
    }
}
