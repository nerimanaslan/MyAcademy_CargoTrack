using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CargoTrack.DTO.DTOs.AuditLogDtos;

namespace CargoTrack.Business.Services.AuditLogs
{
    public interface IAuditLogService
    {
        Task LogAsync(string actionType, string entityName, string entityId, string? oldValue, string? newValue, string description, Guid? userId = null, string? userName = null, string? ipAddress = null);
        Task<List<ResultAuditLogDto>> GetAllLogsAsync(string? entityName = null, string? search = null);
    }
}
