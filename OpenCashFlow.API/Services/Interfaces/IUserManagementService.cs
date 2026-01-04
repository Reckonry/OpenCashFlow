using global::Shared.DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenCashFlow.API.Services.Interfaces
{
    /// <summary>
    /// Service per la gestione admin degli utenti.
    /// </summary>
    public interface IUserManagementService
    {
        /// <summary>
        /// Ottiene lista paginata di utenti con filtri.
        /// </summary>
        Task<(List<User_List_DTO> Users, int TotalCount)> GetUsersAsync(User_Filter_DTO filter, CancellationToken cancellationToken = default);

        /// <summary>
        /// Ottiene dettaglio utente con audit log.
        /// </summary>
        Task<User_Detail_DTO> GetUserDetailAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Crea nuovo utente.
        /// </summary>
        Task<User_Detail_DTO> CreateUserAsync(User_Create_DTO dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Aggiorna utente esistente.
        /// </summary>
        Task<User_Detail_DTO> UpdateUserAsync(User_Update_DTO dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Blocca accesso utente (lockout).
        /// </summary>
        Task LockUserAsync(Guid userId, DateTime? lockoutEnd = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sblocca accesso utente.
        /// </summary>
        Task UnlockUserAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Reset password utente.
        /// </summary>
        Task ResetPasswordAsync(User_ResetPassword_DTO dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gestione ruoli utente.
        /// </summary>
        Task UpdateUserRolesAsync(User_Roles_DTO dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Ottiene lista di tutti i ruoli disponibili.
        /// </summary>
        Task<List<Role_DTO>> GetRolesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina utente (soft delete se supportato, altrimenti hard delete).
        /// </summary>
        Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
