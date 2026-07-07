namespace OpenCashFlow.Application.UserManagement.Models;

public sealed record AdminUserDetail(
    Guid UserId,
    string Username,
    string Email,
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    bool IsActive,
    bool IsLocked,
    DateTime? LockoutEnd,
    DateTime CreatedDate,
    DateTime? LastLoginDate,
    string[] Roles,
    Guid? TenantId,
    string? CompanyName,
    int AccessFailedCount,
    bool EmailConfirmed,
    bool PhoneNumberConfirmed,
    bool TwoFactorEnabled,
    IReadOnlyList<AdminUserAuditEntry> AuditLog);

