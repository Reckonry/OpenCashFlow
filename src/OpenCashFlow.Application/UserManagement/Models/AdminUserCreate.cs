namespace OpenCashFlow.Application.UserManagement.Models;

public sealed record AdminUserCreate(
    string Username,
    string Email,
    string Password,
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    Guid TenantId,
    string[] Roles,
    bool SendWelcomeEmail,
    bool RequirePasswordChange,
    Guid CurrentUserId);

