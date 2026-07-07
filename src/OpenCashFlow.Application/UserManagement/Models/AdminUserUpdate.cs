namespace OpenCashFlow.Application.UserManagement.Models;

public sealed record AdminUserUpdate(
    Guid UserId,
    string? Email,
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    bool? IsActive,
    Guid? TenantId,
    Guid CurrentUserId);

