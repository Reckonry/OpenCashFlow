using Microsoft.EntityFrameworkCore;
using OpenCashFlow.Application.Auth.Ports;
using OpenCashFlow.Infrastructure.Persistence;

namespace OpenCashFlow.Infrastructure.Auth;

public sealed class UserPasswordWriter(ApplicationDbContext db) : IUserPasswordWriter
{
    public async Task UpdatePasswordHashAsync(Guid userId, string passwordHash, CancellationToken cancellationToken = default)
    {
        var user = await db.AspNetUser_DS.FirstOrDefaultAsync(u => u.UserID == userId, cancellationToken)
            ?? throw new InvalidOperationException("Utente non trovato");

        user.PasswordHash = passwordHash;
        user.PasswordResetToken = null;
        user.PasswordResetTokenValidUntil = null;
        user.UserMustChangePassword = false;
        db.AspNetUser_DS.Update(user);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemovePasswordChangeRequirementAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await db.AspNetUser_DS.FirstOrDefaultAsync(u => u.UserID == userId, cancellationToken)
            ?? throw new InvalidOperationException("Utente non trovato");

        user.UserMustChangePassword = false;
        db.AspNetUser_DS.Update(user);
        await db.SaveChangesAsync(cancellationToken);
    }
}
