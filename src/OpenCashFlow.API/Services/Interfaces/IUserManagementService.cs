using global::Shared.DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenCashFlow.API.Services.Interfaces
{
    /// <summary>
    /// Service for admin user management.
    /// </summary>
    public interface IUserManagementService
    {
        /// <summary>
        /// Gets a paginated list of users with filters.
        /// </summary>
        Task<(List<User_List_DTO> Users, int TotalCount)> GetUsersAsync(User_Filter_DTO filter, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets user details with audit log.
        /// </summary>
        Task<User_Detail_DTO> GetUserDetailAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new user.
        /// </summary>
        Task<User_Detail_DTO> CreateUserAsync(User_Create_DTO dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing user.
        /// </summary>
        Task<User_Detail_DTO> UpdateUserAsync(User_Update_DTO dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Locks user access (lockout).
        /// </summary>
        Task LockUserAsync(Guid userId, DateTime? lockoutEnd = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Unlocks user access.
        /// </summary>
        Task UnlockUserAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Resets a user password.
        /// </summary>
        Task ResetPasswordAsync(User_ResetPassword_DTO dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Manages user roles.
        /// </summary>
        Task UpdateUserRolesAsync(User_Roles_DTO dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a list of all available roles.
        /// </summary>
        Task<List<Role_DTO>> GetRolesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a user (soft delete if supported, otherwise hard delete).
        /// </summary>
        Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
