using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoTrack.Entity.Entities;
using CargoTrack.Entity.Entities.Common;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CargoTrack.DataAccess.Context
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<About> Abouts { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Cargo> Cargos { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<ContactInfo> ContactInfos { get; set; }
        public DbSet<CargoMovement> CargoMovements { get; set; }
        public DbSet<TransferCenter> TransferCenters { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<DeliveryException> DeliveryExceptions { get; set; }
        public DbSet<CargoPrice> CargoPrices { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<BaseEntity>();
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    if (entry.Entity.CreatedDate == default)
                    {
                        entry.Entity.CreatedDate = DateTime.Now;
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedDate = DateTime.Now;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Soft delete query filters
            modelBuilder.Entity<Cargo>().HasIndex(c => c.TrackCode).IsUnique();
            modelBuilder.Entity<Cargo>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Branch>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<CargoMovement>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<TransferCenter>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Employee>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Customer>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Delivery>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<DeliveryException>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<CargoPrice>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<AuditLog>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<City>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Address>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<About>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<ContactInfo>().HasQueryFilter(e => !e.IsDeleted);

            // Cargo Sender / Receiver
            modelBuilder.Entity<Cargo>()
                .HasOne(c => c.Sender)
                .WithMany(m => m.SentCargos)
                .HasForeignKey(c => c.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cargo>()
                .HasOne(c => c.Receiver)
                .WithMany(m => m.ReceivedCargos)
                .HasForeignKey(c => c.ReceiverId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Cargo Origin / Destination Branches
            modelBuilder.Entity<Cargo>()
                .HasOne(c => c.OriginBranch)
                .WithMany(m => m.OriginCargos)
                .HasForeignKey(c => c.OriginBranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cargo>()
                .HasOne(c => c.DestinationBranch)
                .WithMany(m => m.DestinationCargos)
                .HasForeignKey(c => c.DestinationBranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cargo>()
                .HasOne(c => c.CurrentBranch)
                .WithMany()
                .HasForeignKey(c => c.CurrentBranchId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cargo>()
                .HasOne(c => c.CurrentTransferCenter)
                .WithMany()
                .HasForeignKey(c => c.CurrentTransferCenterId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cargo>()
                .HasOne(c => c.DeliveredByEmployee)
                .WithMany()
                .HasForeignKey(c => c.DeliveredByEmployeeId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Cargo movements
            modelBuilder.Entity<CargoMovement>()
                .HasOne(m => m.Cargo)
                .WithMany(c => c.Movements)
                .HasForeignKey(m => m.CargoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CargoMovement>()
                .HasOne(m => m.Branch)
                .WithMany(b => b.Movements)
                .HasForeignKey(m => m.BranchId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CargoMovement>()
                .HasOne(m => m.TransferCenter)
                .WithMany(t => t.Movements)
                .HasForeignKey(m => m.TransferCenterId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CargoMovement>()
                .HasOne(m => m.Employee)
                .WithMany(e => e.Movements)
                .HasForeignKey(m => m.EmployeeId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Employees
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Branch)
                .WithMany(b => b.Employees)
                .HasForeignKey(e => e.BranchId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.TransferCenter)
                .WithMany(t => t.Employees)
                .HasForeignKey(e => e.TransferCenterId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.User)
                .WithMany(u => u.Employees)
                .HasForeignKey(e => e.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // AppUser branch
            modelBuilder.Entity<AppUser>()
                .HasOne(u => u.Branch)
                .WithMany(b => b.Users)
                .HasForeignKey(u => u.BranchId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Deliveries
            modelBuilder.Entity<Delivery>()
                .HasOne(d => d.Cargo)
                .WithMany(c => c.Deliveries)
                .HasForeignKey(d => d.CargoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Delivery>()
                .HasOne(d => d.DeliveredByEmployee)
                .WithMany(e => e.Deliveries)
                .HasForeignKey(d => d.DeliveredByEmployeeId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Delivery Exceptions
            modelBuilder.Entity<DeliveryException>()
                .HasOne(de => de.Cargo)
                .WithMany(c => c.Exceptions)
                .HasForeignKey(de => de.CargoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DeliveryException>()
                .HasOne(de => de.RecordedByEmployee)
                .WithMany(e => e.Exceptions)
                .HasForeignKey(de => de.RecordedByEmployeeId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Customer
            modelBuilder.Entity<Customer>()
                .HasOne(cu => cu.User)
                .WithMany(u => u.Customers)
                .HasForeignKey(cu => cu.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
