using System;
using System.Collections.Generic;
using CargoTrack.DTO.DTOs.Cargos;

namespace CargoTrack.DTO.DTOs.DashboardDtos
{
    public class AdminDashboardDto
    {
        public int TotalCargos { get; set; }
        public int ActiveCargos { get; set; }
        public int DelayedCargos { get; set; }
        public double DeliverySuccessRate { get; set; }
        public int ActiveBranchesCount { get; set; }
        public int ActiveEmployeesCount { get; set; }

        public List<string> TrafficDays { get; set; } = new List<string>();
        public List<int> TrafficCounts { get; set; } = new List<int>();

        public List<string> StatusLabels { get; set; } = new List<string>();
        public List<int> StatusCounts { get; set; } = new List<int>();

        public List<BusiestBranchDto> Top5Branches { get; set; } = new List<BusiestBranchDto>();
        public List<ResultCargoDto> RecentCargos { get; set; } = new List<ResultCargoDto>();
    }

    public class BusiestBranchDto
    {
        public string BranchName { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
        public int CargoCount { get; set; }
        public double Percentage { get; set; }
    }

    public class ManagerDashboardDto
    {
        public string BranchName { get; set; } = string.Empty;
        public Guid BranchId { get; set; }
        public int TodayIncoming { get; set; }
        public int TodayOutgoing { get; set; }
        public int OutForDeliveryCount { get; set; }
        public int DeliveredCount { get; set; }
        public int DelayedCount { get; set; }
        public int ProblematicCount { get; set; } // exceptions / failed attempts

        public List<ResultCargoDto> ActionRequiredCargos { get; set; } = new List<ResultCargoDto>();
        public List<CargoMovementDto> RecentMovements { get; set; } = new List<CargoMovementDto>();
    }

    public class UserDashboardDto
    {
        public string UserFullName { get; set; } = string.Empty;
        public int ActiveShipmentsCount { get; set; }
        public int DeliveredShipmentsCount { get; set; }
        public int TotalShipmentsCount { get; set; }

        public List<ResultCargoDto> ActiveCargos { get; set; } = new List<ResultCargoDto>();
        public List<ResultCargoDto> PastCargos { get; set; } = new List<ResultCargoDto>();
        public List<UserCargoMovementDto> RecentMovements { get; set; } = new List<UserCargoMovementDto>();
    }

    public class UserCargoMovementDto
    {
        public string TrackCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class PerformanceReportDto
    {
        public string TopOriginBranch { get; set; } = string.Empty;
        public int TopOriginCount { get; set; }

        public string TopDestinationBranch { get; set; } = string.Empty;
        public int TopDestinationCount { get; set; }

        public double AverageDeliveryTimeHours { get; set; }
        public double OnTimeDeliveryRate { get; set; }
        public double ReturnRate { get; set; }

        public List<BranchVolumeStatDto> BranchDailyVolumes { get; set; } = new List<BranchVolumeStatDto>();
        public List<BranchDelayStatDto> TopDelayedBranches { get; set; } = new List<BranchDelayStatDto>();
        public List<EmployeePerformanceStatDto> EmployeePerformances { get; set; } = new List<EmployeePerformanceStatDto>();
    }

    public class BranchVolumeStatDto
    {
        public string BranchName { get; set; } = string.Empty;
        public int DailyVolume { get; set; }
        public double SuccessRate { get; set; }
    }

    public class BranchDelayStatDto
    {
        public string BranchName { get; set; } = string.Empty;
        public int DelayedCount { get; set; }
        public double DelayRate { get; set; }
    }

    public class EmployeePerformanceStatDto
    {
        public string EmployeeName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public int SuccessfulDeliveries { get; set; }
        public int FailedAttempts { get; set; }
        public int DailyOperationCount { get; set; }
    }
}
