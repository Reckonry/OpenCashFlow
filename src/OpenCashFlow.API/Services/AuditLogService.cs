using OpenCashFlow.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using global::Shared.Data;
using global::Shared.DTOs.Admin;
using global::Shared.Enums;
using global::Shared.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenCashFlow.API.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuditLogService> _logger;
        private readonly IAuthenticationService _authenticationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogService(
            ApplicationDbContext context,
            ILogger<AuditLogService> logger,
            IAuthenticationService authenticationService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _authenticationService = authenticationService;
            _httpContextAccessor = httpContextAccessor;
        }

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
                var httpContext = _httpContextAccessor.HttpContext;
                var userId = _authenticationService.GetUserID();
                var username = httpContext?.User?.Identity?.Name;
                var companyId = _authenticationService.GetTenantID();

                var auditLog = new Admin_AuditLog
                {
                    EventType = eventType.ToString(),
                    Resource = resource,
                    ResourceID = resourceId,
                    Action = action,
                    UserID = userId != Guid.Empty ? userId : null,
                    Username = username,
                    Changes = changes != null ? SerializeSanitizedChanges(changes) : null,
                    IPAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                    UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString(),
                    Timestamp = DateTime.UtcNow,
                    Severity = severity,
                    AdditionalInfo = additionalInfo,
                    TenantID = companyId != Guid.Empty ? companyId : null
                };

                _context.Admin_AuditLog_DS.Add(auditLog);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Audit log created: {EventType} - {Resource} - {Action} by {Username}",
                    eventType, resource, action, username ?? "System");
            }
            catch (Exception ex)
            {
                // Do not let an audit log error block the main operation
                _logger.LogError(ex, "Error creating audit log for {EventType} - {Resource} - {Action}",
                    eventType, resource, action);
            }
        }

        public async Task<(List<AuditLog_List_DTO> Logs, int TotalCount)> GetAuditLogsAsync(
            AuditLog_Filter_DTO filter,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Admin_AuditLog_DS.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.EventType))
            {
                query = query.Where(l => l.EventType == filter.EventType);
            }

            if (!string.IsNullOrWhiteSpace(filter.Resource))
            {
                query = query.Where(l => l.Resource == filter.Resource);
            }

            if (filter.UserID.HasValue)
            {
                query = query.Where(l => l.UserID == filter.UserID.Value);
            }

            if (filter.TenantID.HasValue)
            {
                query = query.Where(l => l.TenantID == filter.TenantID.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.IPAddress))
            {
                query = query.Where(l => l.IPAddress == filter.IPAddress);
            }

            if (!string.IsNullOrWhiteSpace(filter.Severity))
            {
                query = query.Where(l => l.Severity == filter.Severity);
            }

            if (filter.TimestampFrom.HasValue)
            {
                query = query.Where(l => l.Timestamp >= filter.TimestampFrom.Value);
            }

            if (filter.TimestampTo.HasValue)
            {
                query = query.Where(l => l.Timestamp <= filter.TimestampTo.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var searchLower = filter.Search.ToLower();
                query = query.Where(l =>
                    l.Action.ToLower().Contains(searchLower) ||
                    (l.Username != null && l.Username.ToLower().Contains(searchLower)) ||
                    (l.Changes != null && l.Changes.ToLower().Contains(searchLower))
                );
            }

            // Count total
            var totalCount = await query.CountAsync(cancellationToken);

            // Sorting
            query = filter.SortBy switch
            {
                "EventType" => filter.SortDescending ? query.OrderByDescending(l => l.EventType) : query.OrderBy(l => l.EventType),
                "Resource" => filter.SortDescending ? query.OrderByDescending(l => l.Resource) : query.OrderBy(l => l.Resource),
                "Action" => filter.SortDescending ? query.OrderByDescending(l => l.Action) : query.OrderBy(l => l.Action),
                "Username" => filter.SortDescending ? query.OrderByDescending(l => l.Username) : query.OrderBy(l => l.Username),
                _ => filter.SortDescending ? query.OrderByDescending(l => l.Timestamp) : query.OrderBy(l => l.Timestamp)
            };

            // Pagination
            var logs = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(l => new AuditLog_List_DTO
                {
                    AuditLogID = l.AuditLogID,
                    EventType = l.EventType,
                    Resource = l.Resource,
                    ResourceID = l.ResourceID,
                    Action = l.Action,
                    UserID = l.UserID,
                    Username = l.Username,
                    IPAddress = l.IPAddress,
                    Timestamp = l.Timestamp,
                    Severity = l.Severity,
                    TenantID = l.TenantID
                })
                .ToListAsync(cancellationToken);

            return (logs, totalCount);
        }

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

        public async Task<AuditLog_Detail_DTO> GetAuditLogDetailAsync(
            Guid auditLogId,
            CancellationToken cancellationToken = default)
        {
            var log = await _context.Admin_AuditLog_DS
                .FirstOrDefaultAsync(l => l.AuditLogID == auditLogId, cancellationToken)
                ?? throw new KeyNotFoundException($"Audit log {auditLogId} not found");

            return new AuditLog_Detail_DTO
            {
                AuditLogID = log.AuditLogID,
                EventType = log.EventType,
                Resource = log.Resource,
                ResourceID = log.ResourceID,
                Action = log.Action,
                UserID = log.UserID,
                Username = log.Username,
                Changes = log.Changes,
                IPAddress = log.IPAddress,
                UserAgent = log.UserAgent,
                Timestamp = log.Timestamp,
                Severity = log.Severity,
                AdditionalInfo = log.AdditionalInfo,
                TenantID = log.TenantID
            };
        }

        public async Task<byte[]> ExportAuditLogAsync(
            AuditLog_Filter_DTO filter,
            CancellationToken cancellationToken = default)
        {
            // Get all logs matching filter (no pagination for export)
            filter.Page = 1;
            filter.PageSize = int.MaxValue;
            var (logs, _) = await GetAuditLogsAsync(filter, cancellationToken);

            // Generate CSV
            var csv = new StringBuilder();
            csv.AppendLine("Timestamp,EventType,Resource,ResourceID,Action,Username,IPAddress,Severity");

            foreach (var log in logs)
            {
                csv.AppendLine($"\"{log.Timestamp:yyyy-MM-dd HH:mm:ss}\",\"{log.EventType}\",\"{log.Resource}\",\"{log.ResourceID}\",\"{log.Action}\",\"{log.Username}\",\"{log.IPAddress}\",\"{log.Severity}\"");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }
    }
}
