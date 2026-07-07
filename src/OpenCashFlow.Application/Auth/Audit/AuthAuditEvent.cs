namespace OpenCashFlow.Application.Auth.Audit;

public sealed record AuthAuditEvent(
    string EventType,
    string Action,
    string Resource,
    Guid? UserId,
    Guid? TenantId,
    string? Username,
    string? IpAddress,
    string? UserAgent,
    string Severity,
    string? AdditionalInfo,
    DateTime TimestampUtc);
