using Microsoft.EntityFrameworkCore;
using OpenCashFlow.Application.Auth.Models;
using OpenCashFlow.Application.Auth.Ports;
using OpenCashFlow.Infrastructure.Persistence;

namespace OpenCashFlow.Infrastructure.Auth;

public sealed class AuthUserReader(ApplicationDbContext db) : IAuthUserReader
{
    public async Task<AuthenticatedUserResult?> GetByUsernameEmailOrPhoneAsync(string userInput, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userInput))
        {
            return null;
        }

        var user = await db.AspNetUser_DS
            .AsNoTracking()
            .Include(u => u.Roles)
                .ThenInclude(r => r.AspNetRole)
            .FirstOrDefaultAsync(u => u.UserName == userInput || u.Email == userInput || u.PhoneNumber == userInput, cancellationToken);

        return user is null ? null : AuthMapping.MapUser(user);
    }

    public async Task<AuthenticatedUserResult?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await db.AspNetUser_DS
            .AsNoTracking()
            .Include(u => u.Roles)
                .ThenInclude(r => r.AspNetRole)
            .FirstOrDefaultAsync(u => u.UserID == userId, cancellationToken);

        return user is null ? null : AuthMapping.MapUser(user);
    }

    public async Task<AuthenticatedUserResult?> GetByTenantAndPinAsync(Guid tenantId, string pin, CancellationToken cancellationToken = default)
    {
        var staffList = await db.Company_Staff_DS
            .AsNoTracking()
            .Include(cs => cs.User)!
                .ThenInclude(u => u!.Roles)
                    .ThenInclude(r => r.AspNetRole)
            .Where(cs => cs.TenantID == tenantId && !cs.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var staff in staffList)
        {
            var user = staff.User;
            if (user is null || string.IsNullOrWhiteSpace(user.PasswordSalt) || string.IsNullOrWhiteSpace(user.QuickLoginPinHash))
            {
                continue;
            }

            if (PasswordHasher.HashPasswordArgon2(pin, user.PasswordSalt) == user.QuickLoginPinHash)
            {
                return AuthMapping.MapUser(user);
            }
        }

        return null;
    }

    public async Task<Guid?> GetUserTenantIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var staff = await db.Company_Staff_DS
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserID == userId && !s.IsDeleted, cancellationToken);

        return staff?.TenantID;
    }

    public Task<string?> GetCompanySecretAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return db.Company_DS
            .AsNoTracking()
            .Where(c => c.TenantID == tenantId)
            .Select(c => c.CompanySecret)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<bool> CompanyExistsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return db.Company_DS.AsNoTracking().AnyAsync(c => c.TenantID == tenantId, cancellationToken);
    }

    public Task<bool> UserExistsAsync(string usernameOrEmail, CancellationToken cancellationToken = default)
    {
        var value = usernameOrEmail.Trim();
        return db.AspNetUser_DS
            .AsNoTracking()
            .AnyAsync(u => u.UserName == value || u.Email == value, cancellationToken);
    }

    public async Task<IReadOnlyList<AuthClaimResult>> GetUserClaimsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var claims = await db.AspNetUserClaim_DS
            .AsNoTracking()
            .Where(c => c.UserID == userId && c.ClaimType != null && c.ClaimValue != null)
            .Select(c => new AuthClaimResult(c.ClaimType!, c.ClaimValue!))
            .ToListAsync(cancellationToken);

        return claims;
    }
}
