namespace OpenCashFlow.Application.AuditLog.Models;

public sealed record AuditLogListItem(
    Guid AuditLogId,
    string EventType,
    string Resource,
    string? ResourceId,
    string Action,
    Guid? UserId,
    string? Username,
    string? IpAddress,
    DateTime Timestamp,
    string? Severity,
    Guid? TenantId);

