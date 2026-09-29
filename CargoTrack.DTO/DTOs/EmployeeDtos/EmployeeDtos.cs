using System;

namespace CargoTrack.DTO.DTOs.EmployeeDtos
{
    public class ResultEmployeeDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Title { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public Guid? TransferCenterId { get; set; }
        public string? TransferCenterName { get; set; }
        public bool IsActive { get; set; }
        public int TotalDeliveries { get; set; }
    }

    public class CreateEmployeeDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? TransferCenterId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateEmployeeDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? TransferCenterId { get; set; }
        public bool IsActive { get; set; }
    }
}
