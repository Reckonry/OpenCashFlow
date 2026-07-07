using Microsoft.EntityFrameworkCore;
using OpenCashFlow.Application.Employees.Models;
using OpenCashFlow.Application.Employees.Ports;
using OpenCashFlow.Infrastructure.Persistence;
using OpenCashFlow.Infrastructure.Persistence.Entities;
using OpenCashFlow.Infrastructure.Persistence.Entities.Identity;

namespace OpenCashFlow.Infrastructure.Employees;

public sealed class EmployeeWriter(ApplicationDbContext db) : IEmployeeWriter
{
    public async Task<EmployeeDetailResult?> CreateAsync(EmployeeWriteDraft employee, CancellationToken cancellationToken = default)
    {
        var user = ToUser(employee);
        var staff = ToStaff(employee);

        await db.AspNetUser_DS.AddAsync(user, cancellationToken);
        await db.Company_Staff_DS.AddAsync(staff, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        if (employee.SelectedRoleID.HasValue)
        {
            await EnsureSingleVisibleRoleAsync(employee.UserID, employee.SelectedRoleID.Value, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }

        return await ReadDetailAsync(employee.UserID, employee.TenantID, cancellationToken);
    }

    public async Task<bool> UpdateAsync(EmployeeWriteDraft employee, CancellationToken cancellationToken = default)
    {
        var user = await db.AspNetUser_DS.FirstOrDefaultAsync(u => u.UserID == employee.UserID, cancellationToken);
        var staff = await db.Company_Staff_DS.FirstOrDefaultAsync(s => s.UserID == employee.UserID && s.TenantID == employee.TenantID, cancellationToken);

        if (user is null || staff is null)
        {
            return false;
        }

        ApplyUser(employee, user);
        ApplyStaff(employee, staff);

        if (employee.SelectedRoleID.HasValue)
        {
            await EnsureSingleVisibleRoleAsync(employee.UserID, employee.SelectedRoleID.Value, cancellationToken);
        }
        else if (employee.RoleIDs.Count > 0)
        {
            await SyncRolesAsync(employee.UserID, employee.RoleIDs, cancellationToken);
        }

        db.AspNetUser_DS.Update(user);
        db.Company_Staff_DS.Update(staff);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SoftDeleteAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var user = await db.AspNetUser_DS.FirstOrDefaultAsync(u => u.UserID == userId, cancellationToken);
        var companyStaff = await db.Company_Staff_DS
            .FirstOrDefaultAsync(cs => cs.UserID == userId && cs.TenantID == tenantId && !cs.IsDeleted, cancellationToken);

        if (user is null || companyStaff is null)
        {
            return false;
        }

        user.IsDeleted = true;
        user.IsApproved = false;
        user.Email = $"deleted_{user.UserID}@deleted.local";
        user.UserName = $"deleted_{user.UserID:N}";
        user.UserFirstName = "Deleted";
        user.UserLastName = "User";
        user.PhoneNumber = null;
        user.PhoneNumberPrefix = null;
        user.PasswordHash = "DELETED_ACCOUNT";
        user.PasswordSalt = "DELETED_ACCOUNT";
        user.QuickLoginPinHash = null;
        user.PasswordResetToken = null;
        user.PasswordResetTokenValidUntil = null;

        companyStaff.IsDeleted = true;

        db.AspNetUser_DS.Update(user);
        db.Company_Staff_DS.Update(companyStaff);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task UpdateMyProfileAsync(Guid userId, EmployeeProfileUpdate model, CancellationToken cancellationToken = default)
    {
        var user = await db.AspNetUser_DS.FirstOrDefaultAsync(u => u.UserID == userId, cancellationToken);
        if (user is null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(model.UserFirstName))
        {
            user.UserFirstName = model.UserFirstName;
        }

        user.UserLastName = model.UserLastName;

        if (!string.IsNullOrWhiteSpace(model.Email))
        {
            user.Email = model.Email;
        }

        user.PhoneNumberPrefix = model.PhoneNumberPrefix;
        user.PhoneNumber = model.PhoneNumber;

        if (!string.IsNullOrWhiteSpace(model.Language))
        {
            user.Language = model.Language;
        }

        if (!string.IsNullOrWhiteSpace(model.Country))
        {
            user.Country = model.Country;
        }

        user.Timezone = model.Timezone;

        db.AspNetUser_DS.Update(user);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdatePinHashAsync(Guid userId, string pinHash, CancellationToken cancellationToken = default)
    {
        var user = await db.AspNetUser_DS.FirstOrDefaultAsync(u => u.UserID == userId, cancellationToken)
            ?? throw new InvalidOperationException("Utente non trovato");

        user.QuickLoginPinHash = pinHash;
        db.AspNetUser_DS.Update(user);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static AspNetUser ToUser(EmployeeWriteDraft employee)
    {
        var user = new AspNetUser
        {
            UserID = employee.UserID,
            UserName = employee.UserName,
            UserAvatar = employee.UserAvatar,
            Language = employee.Language,
            Country = employee.Country,
            Timezone = employee.Timezone,
            UserTitle = employee.UserTitle,
            UserFirstName = employee.UserFirstName,
            UserMiddleName = employee.UserMiddleName,
            UserLastName = employee.UserLastName,
            Email = employee.Email,
            EmailConfirmed = employee.EmailConfirmed,
            PhoneNumberPrefix = employee.PhoneNumberPrefix,
            PhoneNumber = employee.PhoneNumber,
            PhoneNumberConfirmed = employee.PhoneNumberConfirmed,
            Gender = employee.Gender,
            Pronouns = employee.Pronouns,
            DoB = employee.DoB,
            PoB = employee.PoB,
            SoB = employee.SoB,
            CoB = employee.CoB,
            Nationality = employee.Nationality,
            PrivacyPolicyAcepted = employee.PrivacyPolicyAcepted,
            PrivacyPolicyVersion = employee.PrivacyPolicyVersion,
            PrivacyPolicyAcceptedDate = employee.PrivacyPolicyAcceptedDate,
            LockoutEnd = employee.LockoutEnd,
            LockoutEnabled = employee.LockoutEnabled,
            IsApproved = employee.IsApproved,
            AccessFailedCount = employee.AccessFailedCount,
            FailedPasswordAnswerAttemptCount = employee.FailedPasswordAnswerAttemptCount,
            PasswordHash = employee.PasswordHash,
            PasswordSalt = employee.PasswordSalt,
            QuickLoginPinHash = employee.QuickLoginPinHash,
            UserMustChangePassword = employee.UserMustChangePassword,
            PasswordQuestion = employee.PasswordQuestion,
            PasswordAnswer = employee.PasswordAnswer,
            IsDeleted = employee.IsDeleted,
            IsDeletedBy = employee.IsDeletedBy,
            IsDeletedWhy = employee.IsDeletedWhy,
            CreatedBy = employee.CreatedBy,
            DateIns = employee.DateIns,
            EditedBy = employee.EditedBy,
            DateEdit = employee.DateEdit
        };
        return user;
    }

    private static Company_Staff ToStaff(EmployeeWriteDraft employee)
    {
        var staff = new Company_Staff
        {
            TenantID = employee.TenantID,
            UserID = employee.UserID
        };
        ApplyStaff(employee, staff);
        return staff;
    }

    private static void ApplyUser(EmployeeWriteDraft employee, AspNetUser user)
    {
        user.UserName = employee.UserName;
        user.UserAvatar = employee.UserAvatar;
        user.Language = employee.Language;
        user.Country = employee.Country;
        user.Timezone = employee.Timezone;
        user.UserTitle = employee.UserTitle;
        user.UserFirstName = employee.UserFirstName;
        user.UserMiddleName = employee.UserMiddleName;
        user.UserLastName = employee.UserLastName;
        user.Email = employee.Email;
        user.EmailConfirmed = employee.EmailConfirmed;
        user.PhoneNumberPrefix = employee.PhoneNumberPrefix;
        user.PhoneNumber = employee.PhoneNumber;
        user.PhoneNumberConfirmed = employee.PhoneNumberConfirmed;
        user.Gender = employee.Gender;
        user.Pronouns = employee.Pronouns;
        user.DoB = employee.DoB;
        user.PoB = employee.PoB;
        user.SoB = employee.SoB;
        user.CoB = employee.CoB;
        user.Nationality = employee.Nationality;
        user.PrivacyPolicyAcepted = employee.PrivacyPolicyAcepted;
        user.PrivacyPolicyVersion = employee.PrivacyPolicyVersion;
        user.PrivacyPolicyAcceptedDate = employee.PrivacyPolicyAcceptedDate;
        user.LockoutEnd = employee.LockoutEnd;
        user.LockoutEnabled = employee.LockoutEnabled;
        user.IsApproved = employee.IsApproved;
        user.AccessFailedCount = employee.AccessFailedCount;
        user.FailedPasswordAnswerAttemptCount = employee.FailedPasswordAnswerAttemptCount;
        user.IsDeleted = employee.IsDeleted;
        user.IsDeletedBy = employee.IsDeletedBy;
        user.IsDeletedWhy = employee.IsDeletedWhy;
        user.EditedBy = employee.EditedBy;
        user.DateEdit = employee.DateEdit;
    }

    private static void ApplyStaff(EmployeeWriteDraft employee, Company_Staff staff)
    {
        staff.TimeCost = employee.TimeCost;
        staff.BadgeID = employee.BadgeID;
        staff.OutOfReports = employee.OutOfReports;
        staff.RequireShiftCheckIn = employee.RequireShiftCheckIn;
        staff.LastCheckIn = employee.LastCheckIn;
        staff.LastCheckOut = employee.LastCheckOut;
        staff.Role = employee.Role;
        staff.Department = employee.Department;
        staff.WorkLocation = employee.WorkLocation;
        staff.ContractStartDate = employee.ContractStartDate;
        staff.ContractEndDate = employee.ContractEndDate;
        staff.MonthlySalary = employee.MonthlySalary;
        staff.Bonuses = employee.Bonuses;
        staff.Allowances = employee.Allowances;
        staff.EmploymentType = employee.EmploymentType;
        staff.OvertimeRate = employee.OvertimeRate;
        staff.Skills = employee.Skills;
        staff.SupervisorID = employee.SupervisorID;
        staff.AccessLevel = employee.AccessLevel;
        staff.AuthorizedAreas = employee.AuthorizedAreas;
        staff.InternalNotes = employee.InternalNotes;
        staff.PublicNotes = employee.PublicNotes;
        staff.ExternalSystemReference = employee.ExternalSystemReference;
        staff.SyncStatus = employee.SyncStatus;
        staff.IsDeleted = employee.IsDeleted;
        staff.IsDeletedBy = employee.IsDeletedBy;
        staff.IsDeletedWhy = employee.IsDeletedWhy;
        staff.DateDeleted = employee.DateDeleted;
        staff.CreatedBy = employee.CreatedBy;
        staff.DateIns = employee.DateIns;
        staff.EditedBy = employee.EditedBy;
        staff.DateEdit = employee.DateEdit;
    }

    private async Task EnsureSingleVisibleRoleAsync(Guid userId, Guid selectedRoleId, CancellationToken cancellationToken)
    {
        var visibleAssignments = await db.AspNetUserRole_DS
            .Include(ur => ur.AspNetRole)
            .Where(ur => ur.UserID == userId && ur.AspNetRole != null && ur.AspNetRole.IsVisible)
            .ToListAsync(cancellationToken);

        var toRemove = visibleAssignments.Where(ur => ur.RoleID != selectedRoleId).ToList();
        if (toRemove.Count > 0)
        {
            db.AspNetUserRole_DS.RemoveRange(toRemove);
        }

        var hasSelected = visibleAssignments.Any(ur => ur.RoleID == selectedRoleId) ||
            await db.AspNetUserRole_DS.AnyAsync(ur => ur.UserID == userId && ur.RoleID == selectedRoleId, cancellationToken);

        if (!hasSelected)
        {
            await db.AspNetUserRole_DS.AddAsync(new AspNetUserRole { UserID = userId, RoleID = selectedRoleId }, cancellationToken);
        }
    }

    private async Task SyncRolesAsync(Guid userId, IReadOnlyCollection<Guid> roleIds, CancellationToken cancellationToken)
    {
        var existing = await db.AspNetUserRole_DS.Where(r => r.UserID == userId).ToListAsync(cancellationToken);
        var existingRoleIds = existing.Select(e => e.RoleID).ToHashSet();
        var desiredRoleIds = roleIds.ToHashSet();

        var toAdd = desiredRoleIds.Except(existingRoleIds)
            .Select(rid => new AspNetUserRole { UserID = userId, RoleID = rid })
            .ToList();
        if (toAdd.Count > 0)
        {
            await db.AspNetUserRole_DS.AddRangeAsync(toAdd, cancellationToken);
        }

        var toRemove = existing.Where(e => !desiredRoleIds.Contains(e.RoleID)).ToList();
        if (toRemove.Count > 0)
        {
            db.AspNetUserRole_DS.RemoveRange(toRemove);
        }
    }

    private async Task<EmployeeDetailResult?> ReadDetailAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken)
    {
        var reader = new EmployeeReader(db);
        return await reader.GetEmployeeByIdAsync(userId, tenantId, cancellationToken);
    }
}
