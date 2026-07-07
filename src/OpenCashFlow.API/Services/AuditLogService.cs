using System.Text;
using System.Text.Json;
using OpenCashFlow.API.Services.Interfaces;
using OpenCashFlow.Application.AuditLog;
using OpenCashFlow.Application.AuditLog.Models;
using OpenCashFlow.Contracts.Audit;
using OpenCashFlow.Contracts.DTOs.Admin;

namespace OpenCashFlow.API.Services
{
    public class AuditLogService(
        IAuditLogUseCase auditLogUseCase,
        ILogger<AuditLogService> logger,
        IAuthenticationService authenticationService,
        IHttpContextAccessor httpContextAccessor) : IAuditLogService
    {
        public async Task LogEventAsync(
            AuditEventType eventType,
            string resource,
            string action,
            string? resourceId = null,
            object? changes = null,
            string? additionalInfo = null,
            string? severity = "Info",
            CancellationToken cancellationToken = default)
        {
            try
            {
                var httpContext = httpContextAccessor.HttpContext;
                var userId = authenticationService.GetUserID();
                var tenantId = authenticationService.GetTenantID();

                await auditLogUseCase.WriteAsync(new AuditLogEvent(
                    eventType.ToString(),
                    resource,
                    action,
                    resourceId,
                    changes != null ? SerializeSanitizedChanges(changes) : null,
                    additionalInfo,
                    severity,
                    userId != Guid.Empty ? userId : null,
                    httpContext?.User?.Identity?.Name,
                    tenantId != Guid.Empty ? tenantId : null,
                    httpContext?.Connection?.RemoteIpAddress?.ToString(),
                    httpContext?.Request?.Headers["User-Agent"].ToString(),
                    DateTime.UtcNow), cancellationToken);

                logger.LogInformation("Audit log created: {EventType} - {Resource} - {Action} by {Username}",
                    eventType, resource, action, httpContext?.User?.Identity?.Name ?? "System");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating audit log for {EventType} - {Resource} - {Action}",
                    eventType, resource, action);
            }
        }

        public async Task<(List<AuditLog_List_DTO> Logs, int TotalCount)> GetAuditLogsAsync(
            AuditLog_Filter_DTO filter,
            CancellationToken cancellationToken = default)
        {
            var result = await auditLogUseCase.GetLogsAsync(new AuditLogFilter(
                filter.EventType,
                filter.Resource,
                filter.UserID,
                filter.TenantID,
                filter.IPAddress,
                filter.Severity,
                filter.TimestampFrom,
                filter.TimestampTo,
                filter.Search,
                filter.Page,
                filter.PageSize,
                filter.SortBy,
                filter.SortDescending), cancellationToken);

            return (result.Logs.Select(ToDto).ToList(), result.TotalCount);
        }

        public async Task<AuditLog_Detail_DTO> GetAuditLogDetailAsync(
            Guid auditLogId,
            CancellationToken cancellationToken = default)
        {
            var log = await auditLogUseCase.GetDetailAsync(auditLogId, cancellationToken);
            return ToDto(log);
        }

        public async Task<byte[]> ExportAuditLogAsync(
            AuditLog_Filter_DTO filter,
            CancellationToken cancellationToken = default)
        {
            filter.Page = 1;
            filter.PageSize = int.MaxValue;
            var (logs, _) = await GetAuditLogsAsync(filter, cancellationToken);

            var csv = new StringBuilder();
            csv.AppendLine("Timestamp,EventType,Resource,ResourceID,Action,Username,IPAddress,Severity");

            foreach (var log in logs)
            {
                csv.AppendLine($"\"{log.Timestamp:yyyy-MM-dd HH:mm:ss}\",\"{log.EventType}\",\"{log.Resource}\",\"{log.ResourceID}\",\"{log.Action}\",\"{log.Username}\",\"{log.IPAddress}\",\"{log.Severity}\"");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        private static AuditLog_List_DTO ToDto(AuditLogListItem log) => new()
        {
            AuditLogID = log.AuditLogId,
            EventType = log.EventType,
            Resource = log.Resource,
            ResourceID = log.ResourceId,
            Action = log.Action,
            UserID = log.UserId,
            Username = log.Username,
            IPAddress = log.IpAddress,
            Timestamp = log.Timestamp,
            Severity = log.Severity,
            TenantID = log.TenantId
        };

        private static AuditLog_Detail_DTO ToDto(AuditLogDetail log) => new()
        {
            AuditLogID = log.AuditLogId,
            EventType = log.EventType,
            Resource = log.Resource,
            ResourceID = log.ResourceId,
            Action = log.Action,
            UserID = log.UserId,
            Username = log.Username,
            Changes = log.Changes,
            IPAddress = log.IpAddress,
            UserAgent = log.UserAgent,
            Timestamp = log.Timestamp,
            Severity = log.Severity,
            AdditionalInfo = log.AdditionalInfo,
            TenantID = log.TenantId
        };

        private static string SerializeSanitizedChanges(object changes)
        {
            var json = JsonSerializer.Serialize(changes);
            using var document = JsonDocument.Parse(json);
            var sanitized = SanitizeElement(document.RootElement);
            return JsonSerializer.Serialize(sanitized);
        }

        private static object? SanitizeElement(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Object => element.EnumerateObject()
                    .ToDictionary(
                        property => property.Name,
                        property => IsSensitiveKey(property.Name) ? "***REDACTED***" : SanitizeElement(property.Value)),
                JsonValueKind.Array => element.EnumerateArray().Select(SanitizeElement).ToArray(),
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.TryGetInt64(out var longValue) ? longValue : element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => element.ToString()
            };
        }

        private static bool IsSensitiveKey(string key)
        {
            return key.Contains("password", StringComparison.OrdinalIgnoreCase)
                || key.Contains("token", StringComparison.OrdinalIgnoreCase)
                || key.Contains("pin", StringComparison.OrdinalIgnoreCase)
                || key.Contains("secret", StringComparison.OrdinalIgnoreCase)
                || key.Contains("key", StringComparison.OrdinalIgnoreCase);
        }
    }
}
