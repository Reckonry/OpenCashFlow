using OpenCashFlow.API.Services.Interfaces;
using OpenCashFlow.Application.UserManagement;
using OpenCashFlow.Application.UserManagement.Models;
using OpenCashFlow.Contracts.DTOs.Admin;

namespace OpenCashFlow.API.Services
{
    public class UserManagementService(
        IUserManagementUseCase userManagementUseCase,
        ILogger<UserManagementService> logger,
        IAuthenticationService authenticationService) : IUserManagementService
    {
        public async Task<(List<User_List_DTO> Users, int TotalCount)> GetUsersAsync(
            User_Filter_DTO filter,
            CancellationToken cancellationToken = default)
        {
            var result = await userManagementUseCase.GetUsersAsync(new AdminUserFilter(
                filter.Search,
                filter.TenantID,
                filter.Role,
                filter.IsActive,
                filter.IsLocked,
                filter.EmailConfirmed,
                filter.TwoFactorEnabled,
                filter.CreatedFrom,
                filter.CreatedTo,
                filter.LastLoginFrom,
                filter.LastLoginTo,
                filter.Page,
                filter.PageSize,
                filter.SortBy,
                filter.SortDescending), cancellationToken);

            return (result.Users.Select(ToDto).ToList(), result.TotalCount);
        }

        public async Task<User_Detail_DTO> GetUserDetailAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await userManagementUseCase.GetUserDetailAsync(userId, cancellationToken);
            return ToDto(user);
        }

        public async Task<User_Detail_DTO> CreateUserAsync(User_Create_DTO dto, CancellationToken cancellationToken = default)
        {
            var user = await userManagementUseCase.CreateUserAsync(new AdminUserCreate(
                dto.Username,
                dto.Email,
                dto.Password,
                dto.FirstName,
                dto.LastName,
                dto.PhoneNumber,
                dto.TenantID,
                dto.Roles,
                dto.SendWelcomeEmail,
                dto.RequirePasswordChange,
                authenticationService.GetUserID()), cancellationToken);

            logger.LogInformation("User {Username} created for company {CompanyId}", dto.Username, dto.TenantID);
            return ToDto(user);
        }

        public async Task<User_Detail_DTO> UpdateUserAsync(User_Update_DTO dto, CancellationToken cancellationToken = default)
        {
            var user = await userManagementUseCase.UpdateUserAsync(new AdminUserUpdate(
                dto.UserID,
                dto.Email,
                dto.FirstName,
                dto.LastName,
                dto.PhoneNumber,
                dto.IsActive,
                dto.TenantID,
                authenticationService.GetUserID()), cancellationToken);

            logger.LogInformation("User {UserId} updated", dto.UserID);
            return ToDto(user);
        }

        public Task LockUserAsync(Guid userId, DateTime? lockoutEnd = null, CancellationToken cancellationToken = default)
            => userManagementUseCase.LockUserAsync(userId, lockoutEnd, authenticationService.GetUserID(), cancellationToken);

        public Task UnlockUserAsync(Guid userId, CancellationToken cancellationToken = default)
            => userManagementUseCase.UnlockUserAsync(userId, authenticationService.GetUserID(), cancellationToken);

        public Task ResetPasswordAsync(User_ResetPassword_DTO dto, CancellationToken cancellationToken = default)
            => userManagementUseCase.ResetPasswordAsync(new AdminUserResetPassword(
                dto.UserID,
                dto.NewPassword,
                dto.RequirePasswordChange,
                dto.SendNotificationEmail,
                authenticationService.GetUserID()), cancellationToken);

        public Task UpdateUserRolesAsync(User_Roles_DTO dto, CancellationToken cancellationToken = default)
            => userManagementUseCase.UpdateUserRolesAsync(dto.UserID, dto.Roles, authenticationService.GetUserID(), cancellationToken);

        public async Task<List<Role_DTO>> GetRolesAsync(CancellationToken cancellationToken = default)
        {
            var roles = await userManagementUseCase.GetRolesAsync(cancellationToken);
            return roles.Select(role => new Role_DTO
            {
                RoleName = role.RoleName,
                Description = role.Description,
                UserCount = role.UserCount
            }).ToList();
        }

        public Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
            => userManagementUseCase.DeleteUserAsync(userId, authenticationService.GetUserID(), cancellationToken);

        private static User_List_DTO ToDto(AdminUserListItem user) => new()
        {
            UserID = user.UserId,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            IsLocked = user.IsLocked,
            LockoutEnd = user.LockoutEnd,
            CreatedDate = user.CreatedDate,
            LastLoginDate = user.LastLoginDate,
            Roles = user.Roles,
            AccessFailedCount = user.AccessFailedCount,
            EmailConfirmed = user.EmailConfirmed,
            CompanyName = user.CompanyName,
            TenantID = user.TenantId
        };

        private static User_Detail_DTO ToDto(AdminUserDetail user) => new()
        {
            UserID = user.UserId,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            IsLocked = user.IsLocked,
            LockoutEnd = user.LockoutEnd,
            CreatedDate = user.CreatedDate,
            LastLoginDate = user.LastLoginDate,
            Roles = user.Roles,
            AccessFailedCount = user.AccessFailedCount,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            TwoFactorEnabled = user.TwoFactorEnabled,
            CompanyName = user.CompanyName,
            TenantID = user.TenantId,
            AuditLog = user.AuditLog.Select(audit => new UserAuditEntry_DTO
            {
                Timestamp = audit.Timestamp,
                Action = audit.Action,
                Details = audit.Details,
                PerformedBy = audit.PerformedBy,
                IPAddress = audit.IpAddress
            }).ToList()
        };
    }
}
