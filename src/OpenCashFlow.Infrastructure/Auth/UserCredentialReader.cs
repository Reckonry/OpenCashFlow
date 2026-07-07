using Microsoft.EntityFrameworkCore;
using OpenCashFlow.Application.Auth.Models;
using OpenCashFlow.Application.Auth.Ports;
using OpenCashFlow.Infrastructure.Persistence;

namespace OpenCashFlow.Infrastructure.Auth;

public sealed class UserCredentialReader(ApplicationDbContext db) : IUserCredentialReader
{
    public Task<AuthUserCredential?> GetByEmailOrUserNameAsync(string userInput, CancellationToken cancellationToken = default)
    {
        return db.AspNetUser_DS
            .AsNoTracking()
            .Where(u => !u.IsDeleted && (u.UserName == userInput || u.Email == userInput || u.PhoneNumber == userInput))
            .Select(u => new AuthUserCredential(u.UserID, u.UserName, u.Email, u.UserFirstName, u.PasswordSalt, u.IsApproved, u.LockoutEnabled))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<AuthUserCredential?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return db.AspNetUser_DS
            .AsNoTracking()
            .Where(u => u.UserID == userId && !u.IsDeleted)
            .Select(u => new AuthUserCredential(u.UserID, u.UserName, u.Email, u.UserFirstName, u.PasswordSalt, u.IsApproved, u.LockoutEnabled))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
