using System;

namespace CargoTrack.DTO.DTOs.TransferCenterDtos
{
    public class ResultTransferCenterDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public Guid CityId { get; set; }
        public string CityName { get; set; } = string.Empty;
        public string? AddressDetail { get; set; }
        public int DailyCapacity { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateTransferCenterDto
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public Guid CityId { get; set; }
        public string? AddressDetail { get; set; }
        public int DailyCapacity { get; set; } = 5000;
        public bool IsActive { get; set; } = true;
    }

    public class UpdateTransferCenterDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public Guid CityId { get; set; }
        public string? AddressDetail { get; set; }
        public int DailyCapacity { get; set; }
        public bool IsActive { get; set; }
    }
}
