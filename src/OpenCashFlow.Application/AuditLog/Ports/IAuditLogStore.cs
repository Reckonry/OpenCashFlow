using OpenCashFlow.Application.AuditLog.Models;

namespace OpenCashFlow.Application.AuditLog.Ports;

public interface IAuditLogStore
{
    Task WriteAsync(AuditLogEvent auditEvent, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<AuditLogListItem> Logs, int TotalCount)> GetLogsAsync(AuditLogFilter filter, CancellationToken cancellationToken = default);
    Task<AuditLogDetail> GetDetailAsync(Guid auditLogId, CancellationToken cancellationToken = default);
}

