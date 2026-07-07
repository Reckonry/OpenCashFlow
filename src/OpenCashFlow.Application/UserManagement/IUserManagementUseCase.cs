using OpenCashFlow.Application.UserManagement.Models;

namespace OpenCashFlow.Application.UserManagement;

public interface IUserManagementUseCase
{
    Task<(IReadOnlyList<AdminUserListItem> Users, int TotalCount)> GetUsersAsync(AdminUserFilter filter, CancellationToken cancellationToken = default);
    Task<AdminUserDetail> GetUserDetailAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AdminUserDetail> CreateUserAsync(AdminUserCreate user, CancellationToken cancellationToken = default);
    Task<AdminUserDetail> UpdateUserAsync(AdminUserUpdate user, CancellationToken cancellationToken = default);
    Task LockUserAsync(Guid userId, DateTime? lockoutEnd, Guid currentUserId, CancellationToken cancellationToken = default);
    Task UnlockUserAsync(Guid userId, Guid currentUserId, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(AdminUserResetPassword reset, CancellationToken cancellationToken = default);
    Task UpdateUserRolesAsync(Guid userId, string[] roles, Guid currentUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminRoleSummary>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task DeleteUserAsync(Guid userId, Guid currentUserId, CancellationToken cancellationToken = default);
}

