using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoTrack.DataAccess.Repositories.AuditLogs;
using CargoTrack.DTO.DTOs.AuditLogDtos;
using CargoTrack.Entity.Entities;
using Mapster;

namespace CargoTrack.Business.Services.AuditLogs
{
    public class AuditLogService(IAuditLogRepository _auditLogRepository) : IAuditLogService
    {
        public async Task LogAsync(
            string actionType,
            string entityName,
            string entityId,
            string? oldValue,
            string? newValue,
            string description,
            Guid? userId = null,
            string? userName = null,
            string? ipAddress = null)
        {
            var auditLog = new AuditLog
            {
                ActionType = actionType,
                EntityName = entityName,
                EntityId = entityId,
                OldValue = oldValue,
                NewValue = newValue,
                Description = description,
                UserId = userId,
                UserName = userName ?? "Sistem",
                Timestamp = DateTime.Now,
                IpAddress = ipAddress
            };

            await _auditLogRepository.CreateAsync(auditLog);
        }

        public async Task<List<ResultAuditLogDto>> GetAllLogsAsync(string? entityName = null, string? search = null)
        {
            var logs = await _auditLogRepository.GetListAsync(
                predicate: x =>
                    (string.IsNullOrEmpty(entityName) || x.EntityName == entityName) &&
                    (string.IsNullOrEmpty(search) || x.Description.Contains(search) || (x.UserName != null && x.UserName.Contains(search)) || x.EntityId.Contains(search)),
                orderBy: q => q.OrderByDescending(x => x.Timestamp),
                take: 200
            );

            return logs.Adapt<List<ResultAuditLogDto>>();
        }
    }
}
