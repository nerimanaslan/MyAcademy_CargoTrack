using System;
using CargoTrack.Entity.Entities.Common;

namespace CargoTrack.Entity.Entities
{
    public class DeliveryException : BaseEntity
    {
        public Guid CargoId { get; set; }
        public virtual Cargo Cargo { get; set; }

        public DateTime ExceptionDate { get; set; } = DateTime.Now;
        public string Reason { get; set; } = string.Empty; // "Adres Hatalı", "Alıcı Bulunamadı", "Hasarlı Kargo", "Alıcı Reddetti", "Hava/Ulaşım Engeli"
        public string? Notes { get; set; }
        public int AttemptNumber { get; set; } = 1;

        public Guid? RecordedByEmployeeId { get; set; }
        public virtual Employee? RecordedByEmployee { get; set; }
    }
}
