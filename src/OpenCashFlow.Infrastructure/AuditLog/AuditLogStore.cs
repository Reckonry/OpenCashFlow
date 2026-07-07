using Microsoft.EntityFrameworkCore;
using OpenCashFlow.Application.AuditLog.Models;
using OpenCashFlow.Application.AuditLog.Ports;
using OpenCashFlow.Infrastructure.Persistence;
using OpenCashFlow.Infrastructure.Persistence.Entities.Admin;

namespace OpenCashFlow.Infrastructure.AuditLog;

public sealed class AuditLogStore(ApplicationDbContext context) : IAuditLogStore
{
    public async Task WriteAsync(AuditLogEvent auditEvent, CancellationToken cancellationToken = default)
    {
        context.Admin_AuditLog_DS.Add(new Admin_AuditLog
        {
            EventType = auditEvent.EventType,
            Resource = auditEvent.Resource,
            ResourceID = auditEvent.ResourceId,
            Action = auditEvent.Action,
            UserID = auditEvent.UserId,
            Username = auditEvent.Username,
            Changes = auditEvent.Changes,
            IPAddress = auditEvent.IpAddress,
            UserAgent = auditEvent.UserAgent,
            Timestamp = auditEvent.Timestamp,
            Severity = auditEvent.Severity,
            AdditionalInfo = auditEvent.AdditionalInfo,
            TenantID = auditEvent.TenantId
        });

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<AuditLogListItem> Logs, int TotalCount)> GetLogsAsync(
        AuditLogFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = context.Admin_AuditLog_DS.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.EventType)) query = query.Where(l => l.EventType == filter.EventType);
        if (!string.IsNullOrWhiteSpace(filter.Resource)) query = query.Where(l => l.Resource == filter.Resource);
        if (filter.UserId.HasValue) query = query.Where(l => l.UserID == filter.UserId.Value);
        if (filter.TenantId.HasValue) query = query.Where(l => l.TenantID == filter.TenantId.Value);
        if (!string.IsNullOrWhiteSpace(filter.IpAddress)) query = query.Where(l => l.IPAddress == filter.IpAddress);
        if (!string.IsNullOrWhiteSpace(filter.Severity)) query = query.Where(l => l.Severity == filter.Severity);
        if (filter.TimestampFrom.HasValue) query = query.Where(l => l.Timestamp >= filter.TimestampFrom.Value);
        if (filter.TimestampTo.HasValue) query = query.Where(l => l.Timestamp <= filter.TimestampTo.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchLower = filter.Search.ToLower();
            query = query.Where(l =>
                l.Action.ToLower().Contains(searchLower) ||
                (l.Username != null && l.Username.ToLower().Contains(searchLower)) ||
                (l.Changes != null && l.Changes.ToLower().Contains(searchLower)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = filter.SortBy switch
        {
            "EventType" => filter.SortDescending ? query.OrderByDescending(l => l.EventType) : query.OrderBy(l => l.EventType),
            "Resource" => filter.SortDescending ? query.OrderByDescending(l => l.Resource) : query.OrderBy(l => l.Resource),
            "Action" => filter.SortDescending ? query.OrderByDescending(l => l.Action) : query.OrderBy(l => l.Action),
            "Username" => filter.SortDescending ? query.OrderByDescending(l => l.Username) : query.OrderBy(l => l.Username),
            _ => filter.SortDescending ? query.OrderByDescending(l => l.Timestamp) : query.OrderBy(l => l.Timestamp)
        };

        var logs = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(l => new AuditLogListItem(
                l.AuditLogID,
                l.EventType,
                l.Resource,
                l.ResourceID,
                l.Action,
                l.UserID,
                l.Username,
                l.IPAddress,
                l.Timestamp,
                l.Severity,
                l.TenantID))
            .ToListAsync(cancellationToken);

        return (logs, totalCount);
    }

    public async Task<AuditLogDetail> GetDetailAsync(Guid auditLogId, CancellationToken cancellationToken = default)
    {
        var log = await context.Admin_AuditLog_DS
            .FirstOrDefaultAsync(l => l.AuditLogID == auditLogId, cancellationToken)
            ?? throw new KeyNotFoundException($"Audit log {auditLogId} not found");

        return new AuditLogDetail(
            log.AuditLogID,
            log.EventType,
            log.Resource,
            log.ResourceID,
            log.Action,
            log.UserID,
            log.Username,
            log.Changes,
            log.IPAddress,
            log.UserAgent,
            log.Timestamp,
            log.Severity,
            log.AdditionalInfo,
            log.TenantID);
    }
}

