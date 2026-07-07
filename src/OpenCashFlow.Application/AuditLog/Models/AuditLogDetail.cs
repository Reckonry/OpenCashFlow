namespace OpenCashFlow.Application.AuditLog.Models;

public sealed record AuditLogDetail(
    Guid AuditLogId,
    string EventType,
    string Resource,
    string? ResourceId,
    string Action,
    Guid? UserId,
    string? Username,
    string? Changes,
    string? IpAddress,
    string? UserAgent,
    DateTime Timestamp,
    string? Severity,
    string? AdditionalInfo,
    Guid? TenantId);

