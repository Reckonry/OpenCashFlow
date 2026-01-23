using global::Shared.DTOs.Employees;
using global::Shared.Models;
using global::Shared.Models.Identity;

namespace OpenCashFlow.API.Repositories.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Company_Staff>?> GetEmployeesAsync(Guid TenantID, CancellationToken cancellationToken);
        Task<Company_Staff?> GetEmployeeByIdAsync(Guid UserID, Guid TenantID, CancellationToken cancellationToken);
        Task<AspNetUser?> GetUserAsync(string userInput, CancellationToken cancellationToken);
        Task CreateEmployeeAsync(Employee_Create_DTO employee, CancellationToken cancellationToken);
        Task UpdateEmployeeAsync(Employee_Update_DTO employee, CancellationToken cancellationToken);
        Task UpdateMyProfileAsync(Guid UserID, Employee_MyProfile_Update_DTO model, CancellationToken cancellationToken);
        Task DeleteEmployeeAsync(AspNetUser employee, CancellationToken cancellationToken);


        Task CreateResetTokenAsync(Guid UserID, string token, DateTime expiresAt, CancellationToken cancellationToken);
        Task<bool> HasValidTokenAsync(Guid UserID, string token, CancellationToken cancellationToken);
        Task UpdatePasswordAsync(Guid UserID, string newPassword, CancellationToken cancellationToken);
        Task RemovePasswordChangeRequirementAsync(Guid UserID, CancellationToken cancellationToken);
        Task<Guid?> GetUserIdFromResetTokenAsync(string token, CancellationToken cancellationToken);
        Task InvalidateResetTokenAsync(Guid UserID, string token, CancellationToken cancellationToken);
        Task<AspNetUser?> GetUserByIdAsync(Guid UserID, CancellationToken cancellationToken);
        Task<(bool IsValid, bool IsExpired, AspNetUser? User)> ValidateResetTokenAsync(string token, CancellationToken cancellationToken);

        // Check if a fast login PIN is already used within the company
        Task<bool> IsFastLoginPinInUseAsync(Guid TenantID, string pin, CancellationToken cancellationToken);

        // Check if an email is already used by another user (excluding the current user)
        Task<bool> IsEmailInUseByOtherUserAsync(string email, Guid currentUserId, CancellationToken cancellationToken);

        // Get user by email only (more specific than GetUserAsync)
        Task<AspNetUser?> GetUserByEmailAsync(string email, CancellationToken cancellationToken);

        // Update only the PIN hash for a user
        Task UpdateUserPinAsync(Guid userID, string newPinHash, CancellationToken cancellationToken);
    }
}
