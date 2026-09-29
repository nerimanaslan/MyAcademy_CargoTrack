using System;
using CargoTrack.Entity.Entities.Common;

namespace CargoTrack.Entity.Entities
{
    public class AuditLog : BaseEntity
    {
        public Guid? UserId { get; set; }
        public string? UserName { get; set; }
        public string ActionType { get; set; } = string.Empty; // Create, Update, Delete, StatusChange, Delivery, DeliveryException
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string? IpAddress { get; set; }
    }
}
