using OpenCashFlow.Application.Auth.Models;
using OpenCashFlow.Infrastructure.Persistence.Entities.Identity;

namespace OpenCashFlow.Infrastructure.Auth;

internal static class AuthMapping
{
    public static AuthenticatedUserResult MapUser(AspNetUser user)
    {
        return new AuthenticatedUserResult
        {
            UserID = user.UserID,
            UserName = user.UserName,
            Email = user.Email,
            UserFirstName = user.UserFirstName,
            UserLastName = user.UserLastName,
            FullName = user.EmployeeSurnameName,
            UserAvatar = user.UserAvatar,
            PasswordSalt = user.PasswordSalt,
            PasswordHash = user.PasswordHash,
            IsApproved = user.IsApproved,
            LockoutEnabled = user.LockoutEnabled,
            UserMustChangePassword = user.UserMustChangePassword,
            Roles = user.Roles?
                .Where(r => r.AspNetRole != null && !string.IsNullOrWhiteSpace(r.AspNetRole.RoleName))
                .Select(r => r.AspNetRole!.RoleName)
                .ToList() ?? []
        };
    }
}
