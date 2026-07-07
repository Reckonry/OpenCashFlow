using OpenCashFlow.Application.AuditLog.Models;
using OpenCashFlow.Application.AuditLog.Ports;

namespace OpenCashFlow.Application.AuditLog;

public sealed class AuditLogUseCase(IAuditLogStore store) : IAuditLogUseCase
{
    public Task WriteAsync(AuditLogEvent auditEvent, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(auditEvent.EventType)) throw new ArgumentException("Event type is required.", nameof(auditEvent));
        if (string.IsNullOrWhiteSpace(auditEvent.Resource)) throw new ArgumentException("Resource is required.", nameof(auditEvent));
        if (string.IsNullOrWhiteSpace(auditEvent.Action)) throw new ArgumentException("Action is required.", nameof(auditEvent));

        return store.WriteAsync(auditEvent, cancellationToken);
    }

    public Task<(IReadOnlyList<AuditLogListItem> Logs, int TotalCount)> GetLogsAsync(AuditLogFilter filter, CancellationToken cancellationToken = default)
    {
        var normalized = filter with
        {
            Page = filter.Page < 1 ? 1 : filter.Page,
            PageSize = filter.PageSize < 1 ? 50 : Math.Min(filter.PageSize, 200)
        };

        return store.GetLogsAsync(normalized, cancellationToken);
    }

    public Task<AuditLogDetail> GetDetailAsync(Guid auditLogId, CancellationToken cancellationToken = default)
    {
        if (auditLogId == Guid.Empty) throw new ArgumentException("Audit log id is required.", nameof(auditLogId));
        return store.GetDetailAsync(auditLogId, cancellationToken);
    }
}

