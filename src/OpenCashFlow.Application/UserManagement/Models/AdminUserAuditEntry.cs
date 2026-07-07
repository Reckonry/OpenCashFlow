namespace OpenCashFlow.Application.UserManagement.Models;

public sealed record AdminUserAuditEntry(
    DateTime Timestamp,
    string Action,
    string Details,
    string? PerformedBy,
    string? IpAddress);

