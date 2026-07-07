namespace OpenCashFlow.Application.AuditLog.Models;

public sealed record AuditLogEvent(
    string EventType,
    string Resource,
    string Action,
    string? ResourceId,
    string? Changes,
    string? AdditionalInfo,
    string? Severity,
    Guid? UserId,
    string? Username,
    Guid? TenantId,
    string? IpAddress,
    string? UserAgent,
    DateTime Timestamp);

