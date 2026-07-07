using OpenCashFlow.Application.UserManagement.Models;
using OpenCashFlow.Application.UserManagement.Ports;

namespace OpenCashFlow.Application.UserManagement;

public sealed class UserManagementUseCase(IUserManagementStore store) : IUserManagementUseCase
{
    public Task<(IReadOnlyList<AdminUserListItem> Users, int TotalCount)> GetUsersAsync(AdminUserFilter filter, CancellationToken cancellationToken = default)
    {
        var normalized = filter with
        {
            Page = filter.Page < 1 ? 1 : filter.Page,
            PageSize = filter.PageSize < 1 ? 50 : Math.Min(filter.PageSize, 200)
        };

        return store.GetUsersAsync(normalized, cancellationToken);
    }

    public Task<AdminUserDetail> GetUserDetailAsync(Guid userId, CancellationToken cancellationToken = default)
        => store.GetUserDetailAsync(RequireGuid(userId, nameof(userId)), cancellationToken);

    public Task<AdminUserDetail> CreateUserAsync(AdminUserCreate user, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(user.Username)) throw new ArgumentException("Username is required.", nameof(user));
        if (string.IsNullOrWhiteSpace(user.Email)) throw new ArgumentException("Email is required.", nameof(user));
        if (string.IsNullOrWhiteSpace(user.Password)) throw new ArgumentException("Password is required.", nameof(user));
        RequireGuid(user.TenantId, nameof(user.TenantId));
        RequireGuid(user.CurrentUserId, nameof(user.CurrentUserId));

        return store.CreateUserAsync(user, cancellationToken);
    }

    public Task<AdminUserDetail> UpdateUserAsync(AdminUserUpdate user, CancellationToken cancellationToken = default)
    {
        RequireGuid(user.UserId, nameof(user.UserId));
        RequireGuid(user.CurrentUserId, nameof(user.CurrentUserId));
        return store.UpdateUserAsync(user, cancellationToken);
    }

    public Task LockUserAsync(Guid userId, DateTime? lockoutEnd, Guid currentUserId, CancellationToken cancellationToken = default)
        => store.LockUserAsync(RequireGuid(userId, nameof(userId)), lockoutEnd, RequireGuid(currentUserId, nameof(currentUserId)), cancellationToken);

    public Task UnlockUserAsync(Guid userId, Guid currentUserId, CancellationToken cancellationToken = default)
        => store.UnlockUserAsync(RequireGuid(userId, nameof(userId)), RequireGuid(currentUserId, nameof(currentUserId)), cancellationToken);

    public Task ResetPasswordAsync(AdminUserResetPassword reset, CancellationToken cancellationToken = default)
    {
        RequireGuid(reset.UserId, nameof(reset.UserId));
        RequireGuid(reset.CurrentUserId, nameof(reset.CurrentUserId));
        if (string.IsNullOrWhiteSpace(reset.NewPassword)) throw new ArgumentException("Password is required.", nameof(reset));
        return store.ResetPasswordAsync(reset, cancellationToken);
    }

    public Task UpdateUserRolesAsync(Guid userId, string[] roles, Guid currentUserId, CancellationToken cancellationToken = default)
        => store.UpdateUserRolesAsync(RequireGuid(userId, nameof(userId)), roles ?? Array.Empty<string>(), RequireGuid(currentUserId, nameof(currentUserId)), cancellationToken);

    public Task<IReadOnlyList<AdminRoleSummary>> GetRolesAsync(CancellationToken cancellationToken = default)
        => store.GetRolesAsync(cancellationToken);

    public Task DeleteUserAsync(Guid userId, Guid currentUserId, CancellationToken cancellationToken = default)
        => store.DeleteUserAsync(RequireGuid(userId, nameof(userId)), RequireGuid(currentUserId, nameof(currentUserId)), cancellationToken);

    private static Guid RequireGuid(Guid value, string name)
    {
        if (value == Guid.Empty) throw new ArgumentException($"{name} is required.", name);
        return value;
    }
}

