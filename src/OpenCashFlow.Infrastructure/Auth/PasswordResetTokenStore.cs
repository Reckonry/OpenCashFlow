using Microsoft.EntityFrameworkCore;
using OpenCashFlow.Application.Auth.Models;
using OpenCashFlow.Application.Auth.Ports;
using OpenCashFlow.Infrastructure.Persistence;

namespace OpenCashFlow.Infrastructure.Auth;

public sealed class PasswordResetTokenStore(ApplicationDbContext db) : IPasswordResetTokenStore
{
    public async Task CreateAsync(Guid userId, string token, DateTime expiresAt, CancellationToken cancellationToken = default)
    {
        var user = await db.AspNetUser_DS.FirstOrDefaultAsync(u => u.UserID == userId, cancellationToken)
            ?? throw new InvalidOperationException("Utente non trovato");

        user.PasswordResetToken = token;
        user.PasswordResetTokenValidUntil = expiresAt;
        db.AspNetUser_DS.Update(user);
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> HasValidTokenAsync(Guid userId, string token, CancellationToken cancellationToken = default)
    {
        return db.AspNetUser_DS
            .AsNoTracking()
            .AnyAsync(u => u.UserID == userId && u.PasswordResetToken == token && u.PasswordResetTokenValidUntil > DateTime.UtcNow, cancellationToken);
    }

    public async Task<Guid?> GetUserIdFromTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var user = await db.AspNetUser_DS
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.PasswordResetToken == token && u.PasswordResetTokenValidUntil > DateTime.UtcNow, cancellationToken);

        return user?.UserID;
    }

    public async Task<PasswordResetTokenValidation> ValidateAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return new PasswordResetTokenValidation(false, false, null, null, null, null);
        }

        var user = await db.AspNetUser_DS
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.PasswordResetToken == token, cancellationToken);

        if (user is null)
        {
            return new PasswordResetTokenValidation(false, false, null, null, null, null);
        }

        var isExpired = user.PasswordResetTokenValidUntil is null || user.PasswordResetTokenValidUntil <= DateTime.UtcNow;
        return new PasswordResetTokenValidation(!isExpired, isExpired, user.UserID, user.UserName, user.Email, user.UserFirstName);
    }

    public async Task InvalidateAsync(Guid userId, string token, CancellationToken cancellationToken = default)
    {
        var user = await db.AspNetUser_DS
            .FirstOrDefaultAsync(u => u.UserID == userId && u.PasswordResetToken == token, cancellationToken);

        if (user is null)
        {
            return;
        }

        user.PasswordResetToken = null;
        user.PasswordResetTokenValidUntil = null;
        db.AspNetUser_DS.Update(user);
        await db.SaveChangesAsync(cancellationToken);
    }
}
