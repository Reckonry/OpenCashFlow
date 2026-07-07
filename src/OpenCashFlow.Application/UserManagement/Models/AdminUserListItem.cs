namespace OpenCashFlow.Application.UserManagement.Models;

public sealed record AdminUserListItem(
    Guid UserId,
    string Username,
    string Email,
    string? FirstName,
    string? LastName,
    bool IsActive,
    bool IsLocked,
    DateTime? LockoutEnd,
    DateTime CreatedDate,
    DateTime? LastLoginDate,
    string[] Roles,
    string? CompanyName,
    Guid? TenantId,
    int AccessFailedCount,
    bool EmailConfirmed);

