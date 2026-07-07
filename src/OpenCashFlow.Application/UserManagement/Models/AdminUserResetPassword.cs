namespace OpenCashFlow.Application.UserManagement.Models;

public sealed record AdminUserResetPassword(
    Guid UserId,
    string NewPassword,
    bool RequirePasswordChange,
    bool SendNotificationEmail,
    Guid CurrentUserId);

