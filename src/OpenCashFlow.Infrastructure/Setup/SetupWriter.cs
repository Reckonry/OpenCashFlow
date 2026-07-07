using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenCashFlow.Application.Setup.CompleteSetup;
using OpenCashFlow.Application.Setup.GetSetupStatus;
using OpenCashFlow.Application.Setup.Ports;
using OpenCashFlow.Contracts.Core;
using OpenCashFlow.Infrastructure.Auth;
using OpenCashFlow.Infrastructure.Persistence;
using OpenCashFlow.Infrastructure.Persistence.Entities;
using OpenCashFlow.Infrastructure.Persistence.Entities.Cash;
using OpenCashFlow.Infrastructure.Persistence.Entities.Identity;

namespace OpenCashFlow.Infrastructure.Setup;

public sealed class SetupWriter(ApplicationDbContext db, ILogger<SetupWriter> logger) : ISetupWriter
{
    public async Task<SetupStatusResult> CompleteAsync(CompleteSetupCommand command, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = command.AdminEmail.Trim();
        var now = DateTime.UtcNow;
        var tenantId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid();
        var salt = PasswordHasher.GenerateSalt();

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await EnsureSystemRolesAsync(cancellationToken);

            db.Company_DS.Add(new Company
            {
                TenantID = tenantId,
                CompanyName = command.CompanyName.Trim(),
                MaxUsers = 1000,
                StartingContract = now,
                EndingContract = now.AddYears(100),
                IsActive = true,
                ContractAcepted = true,
                ContractAcceptedDate = now,
                ContractVersion = "self-hosted",
                GdprConsent = true,
                GdprConsentDate = now,
                DefaultLanguage = command.Language.Trim().ToLowerInvariant(),
                DefaultCurrency = command.Currency.Trim().ToUpperInvariant(),
                DefaultTimezone = command.Timezone.Trim(),
                DefaultCountry = command.Country.Trim().ToUpperInvariant(),
                MasterPassword = null,
                CompanySecret = Guid.NewGuid().ToString("N"),
                CreatedBy = adminUserId,
                DateIns = now
            });

            db.AspNetUser_DS.Add(new AspNetUser
            {
                UserID = adminUserId,
                UserName = normalizedEmail,
                Email = normalizedEmail,
                EmailConfirmed = true,
                IsApproved = true,
                UserFirstName = command.AdminFirstName.Trim(),
                UserLastName = string.IsNullOrWhiteSpace(command.AdminLastName) ? null : command.AdminLastName.Trim(),
                Language = command.Language.Trim().ToUpperInvariant(),
                Country = command.Country.Trim().ToUpperInvariant(),
                Timezone = command.Timezone.Trim(),
                PrivacyPolicyAcepted = true,
                PrivacyPolicyAcceptedDate = now,
                PasswordSalt = salt,
                PasswordHash = PasswordHasher.HashPasswordArgon2(command.AdminPassword, salt),
                DateIns = now
            });

            db.AspNetUserRole_DS.Add(new AspNetUserRole { UserID = adminUserId, RoleID = Configuration.CompanyAdminRoleID });
            db.AspNetUserRole_DS.Add(new AspNetUserRole { UserID = adminUserId, RoleID = Configuration.InstanceAdminRoleID });
            db.Company_Staff_DS.Add(new Company_Staff
            {
                TenantID = tenantId,
                UserID = adminUserId,
                CreatedBy = adminUserId,
                DateIns = now
            });
            db.Add(new Company_Contact_Email
            {
                ContactEmailID = Guid.NewGuid(),
                TenantID = tenantId,
                Email = normalizedEmail,
                EmailConfirmed = true,
                CreatedBy = adminUserId,
                DateIns = now
            });
            db.CashBalances.Add(new CashBalance
            {
                CompanyId = tenantId,
                Balance = 0m,
                LastUpdatedUtc = DateTimeOffset.UtcNow
            });

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation("Self-hosted setup completed for company {CompanyId}", tenantId);
            return new SetupStatusResult(RequiresSetup: false, HasCompanies: true, HasAdminUsers: true);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task EnsureSystemRolesAsync(CancellationToken cancellationToken)
    {
        await EnsureRoleAsync(Configuration.CompanyAdminRoleID, Configuration.CompanyAdminRoleName, true, cancellationToken);
        await EnsureRoleAsync(Configuration.EmployeeRoleID, Configuration.EmployeeRoleName, true, cancellationToken);
        await EnsureRoleAsync(Configuration.InstanceAdminRoleID, Configuration.InstanceAdminRoleName, false, cancellationToken);
    }

    private async Task EnsureRoleAsync(Guid roleId, string roleName, bool visible, CancellationToken cancellationToken)
    {
        var existingRole = await db.AspNetRole_DS.FirstOrDefaultAsync(r => r.RoleID == roleId, cancellationToken);
        if (existingRole != null)
        {
            existingRole.RoleName = roleName;
            existingRole.IsVisible = visible;
            return;
        }

        db.AspNetRole_DS.Add(new AspNetRole
        {
            RoleID = roleId,
            RoleName = roleName,
            IsVisible = visible,
            DateIns = DateTime.UtcNow
        });
    }
}
