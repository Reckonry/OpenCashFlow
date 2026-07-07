namespace OpenCashFlow.Application.AuditLog.Models;

public sealed record AuditLogFilter(
    string? EventType,
    string? Resource,
    Guid? UserId,
    Guid? TenantId,
    string? IpAddress,
    string? Severity,
    DateTime? TimestampFrom,
    DateTime? TimestampTo,
    string? Search,
    int Page,
    int PageSize,
    string? SortBy,
    bool SortDescending);

