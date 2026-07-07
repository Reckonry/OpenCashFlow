using Microsoft.EntityFrameworkCore;
using OpenCashFlow.Application.Auth.Models;
using OpenCashFlow.Application.Auth.Ports;
using OpenCashFlow.Contracts.Core;
using OpenCashFlow.Infrastructure.Persistence;
using OpenCashFlow.Infrastructure.Persistence.Entities;
using OpenCashFlow.Infrastructure.Persistence.Entities.Identity;

namespace OpenCashFlow.Infrastructure.Auth;

public sealed class AuthUserWriter(ApplicationDbContext db) : IAuthUserWriter
{
    public async Task<bool> CreateRegisteredCompanyAndUserAsync(RegistrationDraft draft, Guid tenantId, Guid userId, string passwordSalt, string passwordHash, CancellationToken cancellationToken = default)
    {
        var user = new AspNetUser
        {
            UserID = userId,
            UserName = draft.Email,
            Email = draft.Email,
            EmailConfirmed = false,
            UserFirstName = draft.FirstName ?? draft.CompanyName,
            UserLastName = draft.LastName,
            PasswordSalt = passwordSalt,
            PasswordHash = passwordHash,
            PrivacyPolicyAcepted = draft.AcceptPrivacyPolicy,
            PrivacyPolicyAcceptedDate = draft.AcceptPrivacyPolicy ? DateTime.UtcNow : null
        };

        var role = new AspNetUserRole
        {
            UserID = userId,
            RoleID = Configuration.AdministratorRoleID
        };

        var company = new Company
        {
            TenantID = tenantId,
            CompanyName = draft.CompanyName,
            MaxUsers = 1000,
            StartingContract = DateTime.UtcNow,
            EndingContract = DateTime.UtcNow.AddDays(30),
            IsActive = true,
            MasterPassword = Guid.NewGuid().ToString("N"),
            CompanySecret = Guid.NewGuid().ToString("N"),
            CreatedBy = userId
        };

        var contactEmail = new Company_Contact_Email
        {
            ContactEmailID = Guid.NewGuid(),
            TenantID = tenantId,
            Email = draft.Email,
            CreatedBy = userId
        };

        var staff = new Company_Staff
        {
            UserID = userId,
            TenantID = tenantId,
            CreatedBy = userId
        };

        try
        {
            await db.AspNetUser_DS.AddAsync(user, cancellationToken);
            await db.AspNetUserRole_DS.AddAsync(role, cancellationToken);
            await db.Company_DS.AddAsync(company, cancellationToken);
            await db.AddAsync(contactEmail, cancellationToken);
            await db.Company_Staff_DS.AddAsync(staff, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ConfirmAccountAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await db.AspNetUser_DS.FirstOrDefaultAsync(u => u.UserID == userId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        var staff = await db.Company_Staff_DS.FirstOrDefaultAsync(cs => cs.UserID == userId && cs.TenantID == tenantId, cancellationToken);
        if (staff is null)
        {
            return false;
        }

        user.EmailConfirmed = true;
        user.IsApproved = true;
        db.AspNetUser_DS.Update(user);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
