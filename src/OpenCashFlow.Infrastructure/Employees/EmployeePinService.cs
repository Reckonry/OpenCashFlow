using Microsoft.EntityFrameworkCore;
using OpenCashFlow.Application.Employees.Ports;
using OpenCashFlow.Infrastructure.Auth;
using OpenCashFlow.Infrastructure.Persistence;
using System.Security.Cryptography;

namespace OpenCashFlow.Infrastructure.Employees;

public sealed class EmployeePinService(ApplicationDbContext db) : IEmployeePinService
{
    public async Task<string> GenerateUniquePinAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 50;
        for (var i = 0; i < maxAttempts; i++)
        {
            var pin = RandomNumberGenerator.GetInt32(0, 100000).ToString("D5");
            if (!await IsFastLoginPinInUseAsync(tenantId, pin, cancellationToken))
            {
                return pin;
            }
        }

        throw new InvalidOperationException("Unable to generate a unique PIN. Please try again.");
    }

    private async Task<bool> IsFastLoginPinInUseAsync(Guid tenantId, string pin, CancellationToken cancellationToken)
    {
        var userPins = await db.Company_Staff_DS
            .AsNoTracking()
            .Where(cs => cs.TenantID == tenantId && !cs.IsDeleted && cs.User != null && !string.IsNullOrWhiteSpace(cs.User.PasswordSalt))
            .Select(cs => new { cs.User!.PasswordSalt, cs.User.QuickLoginPinHash })
            .ToListAsync(cancellationToken);

        foreach (var user in userPins)
        {
            if (PasswordHasher.HashPasswordArgon2(pin, user.PasswordSalt) == user.QuickLoginPinHash)
            {
                return true;
            }
        }

        return false;
    }
}
