using Microsoft.EntityFrameworkCore;
using OpenCashFlow.Application.Employees.Models;
using OpenCashFlow.Application.Employees.Ports;
using OpenCashFlow.Infrastructure.Persistence;
using OpenCashFlow.Infrastructure.Persistence.Entities;
using OpenCashFlow.Infrastructure.Persistence.Entities.Identity;

namespace OpenCashFlow.Infrastructure.Employees;

public sealed class EmployeeReader(ApplicationDbContext db) : IEmployeeReader
{
    public async Task<IReadOnlyList<EmployeeListItem>> GetEmployeesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var employees = await EmployeeQuery()
            .Where(e => e.TenantID == tenantId && !e.IsDeleted)
            .ToListAsync(cancellationToken);

        return employees.Select(MapList).ToList();
    }

    public async Task<EmployeeDetailResult?> GetEmployeeByIdAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var employee = await EmployeeQuery()
            .FirstOrDefaultAsync(e => e.UserID == userId && e.TenantID == tenantId && !e.IsDeleted, cancellationToken);

        return employee is null ? null : MapDetail(employee);
    }

    public async Task<EmployeeCredentialSnapshot?> GetEmployeeCredentialAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await db.Company_Staff_DS
            .AsNoTracking()
            .Where(e => e.UserID == userId && e.TenantID == tenantId && !e.IsDeleted && e.User != null)
            .Select(e => new EmployeeCredentialSnapshot
            {
                TenantID = e.TenantID,
                UserID = e.User!.UserID,
                UserName = e.User.UserName,
                Email = e.User.Email,
                UserFirstName = e.User.UserFirstName,
                UserLastName = e.User.UserLastName,
                PasswordSalt = e.User.PasswordSalt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await db.AspNetUser_DS
            .AsNoTracking()
            .AnyAsync(u => u.Email == email && !u.IsDeleted, cancellationToken);
    }

    public async Task<bool> EmailInUseByOtherUserAsync(string email, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        return await db.AspNetUser_DS
            .AsNoTracking()
            .AnyAsync(u => u.Email == email && u.UserID != currentUserId && !u.IsDeleted, cancellationToken);
    }

    private IQueryable<Company_Staff> EmployeeQuery()
    {
        return db.Company_Staff_DS
            .AsNoTracking()
            .Include(e => e.User)!.ThenInclude(u => u!.Roles).ThenInclude(ur => ur.AspNetRole)
            .Include(e => e.User)!.ThenInclude(u => u!.AssignedPermissions)
            .Include(e => e.User)!.ThenInclude(u => u!.DeniedPermissions);
    }

    private static EmployeeListItem MapList(Company_Staff staff)
    {
        var user = staff.User ?? throw new InvalidOperationException("Employee user navigation is required.");
        return new EmployeeListItem
        {
            TenantID = staff.TenantID,
            UserID = user.UserID,
            UserName = user.UserName,
            UserAvatar = user.UserAvatar,
            Language = user.Language,
            Country = user.Country,
            TimezoneID = user.Timezone,
            Roles = MapRoles(user.Roles),
            AssignedPermissions = MapAssignedPermissions(user.AssignedPermissions),
            DeniedPermissions = MapDeniedPermissions(user.DeniedPermissions),
            UserTitle = user.UserTitle,
            UserFirstName = user.UserFirstName,
            UserMiddleName = user.UserMiddleName,
            UserLastName = user.UserLastName,
            Email = user.Email,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberPrefix = user.PhoneNumberPrefix,
            PhoneNumber = user.PhoneNumber,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            Gender = user.Gender,
            Pronouns = user.Pronouns,
            PrivacyPolicyAcepted = user.PrivacyPolicyAcepted,
            PrivacyPolicyVersion = user.PrivacyPolicyVersion,
            PrivacyPolicyAcceptedDate = user.PrivacyPolicyAcceptedDate,
            LockoutEnd = user.LockoutEnd,
            LockoutEnabled = user.LockoutEnabled,
            IsApproved = user.IsApproved,
            AccessFailedCount = user.AccessFailedCount,
            FailedPasswordAnswerAttemptCount = user.FailedPasswordAnswerAttemptCount,
            TimeCost = staff.TimeCost,
            BadgeID = staff.BadgeID,
            OutOfReports = staff.OutOfReports,
            RequireShiftCheckIn = staff.RequireShiftCheckIn,
            LastCheckIn = staff.LastCheckIn,
            LastCheckOut = staff.LastCheckOut,
            Role = staff.Role,
            Department = staff.Department,
            WorkLocation = staff.WorkLocation,
            AccessLevel = staff.AccessLevel,
            AuthorizedAreas = staff.AuthorizedAreas
        };
    }

    private static EmployeeDetailResult MapDetail(Company_Staff staff)
    {
        var user = staff.User ?? throw new InvalidOperationException("Employee user navigation is required.");
        return new EmployeeDetailResult
        {
            TenantID = staff.TenantID,
            UserID = user.UserID,
            UserName = user.UserName,
            UserAvatar = user.UserAvatar,
            Language = user.Language,
            Country = user.Country,
            Timezone = user.Timezone,
            Roles = MapRoles(user.Roles),
            AssignedPermissions = MapAssignedPermissions(user.AssignedPermissions),
            DeniedPermissions = MapDeniedPermissions(user.DeniedPermissions),
            UserTitle = user.UserTitle,
            UserFirstName = user.UserFirstName,
            UserMiddleName = user.UserMiddleName,
            UserLastName = user.UserLastName,
            Email = user.Email,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberPrefix = user.PhoneNumberPrefix,
            PhoneNumber = user.PhoneNumber,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            Gender = user.Gender,
            Pronouns = user.Pronouns,
            DoB = user.DoB,
            PoB = user.PoB,
            SoB = user.SoB,
            CoB = user.CoB,
            Nationality = user.Nationality,
            PrivacyPolicyAcepted = user.PrivacyPolicyAcepted,
            PrivacyPolicyVersion = user.PrivacyPolicyVersion,
            PrivacyPolicyAcceptedDate = user.PrivacyPolicyAcceptedDate,
            LockoutEnd = user.LockoutEnd,
            LockoutEnabled = user.LockoutEnabled,
            IsApproved = user.IsApproved,
            AccessFailedCount = user.AccessFailedCount,
            FailedPasswordAnswerAttemptCount = user.FailedPasswordAnswerAttemptCount,
            TimeCost = staff.TimeCost,
            BadgeID = staff.BadgeID,
            OutOfReports = staff.OutOfReports,
            RequireShiftCheckIn = staff.RequireShiftCheckIn,
            LastCheckIn = staff.LastCheckIn,
            LastCheckOut = staff.LastCheckOut,
            Role = staff.Role,
            Department = staff.Department,
            WorkLocation = staff.WorkLocation,
            ContractStartDate = staff.ContractStartDate,
            ContractEndDate = staff.ContractEndDate,
            MonthlySalary = staff.MonthlySalary,
            Bonuses = staff.Bonuses,
            Allowances = staff.Allowances,
            EmploymentType = staff.EmploymentType,
            OvertimeRate = staff.OvertimeRate,
            Skills = staff.Skills,
            SupervisorID = staff.SupervisorID,
            AccessLevel = staff.AccessLevel,
            AuthorizedAreas = staff.AuthorizedAreas,
            InternalNotes = staff.InternalNotes,
            PublicNotes = staff.PublicNotes,
            ExternalSystemReference = staff.ExternalSystemReference,
            SyncStatus = staff.SyncStatus,
            IsDeleted = staff.IsDeleted,
            IsDeletedBy = staff.IsDeletedBy,
            IsDeletedWhy = staff.IsDeletedWhy,
            DateDeleted = staff.DateDeleted,
            CreatedBy = staff.CreatedBy,
            DateIns = staff.DateIns,
            EditedBy = staff.EditedBy,
            DateEdit = staff.DateEdit
        };
    }

    private static List<EmployeeRoleResult> MapRoles(IEnumerable<AspNetUserRole> roles)
    {
        return roles
            .Where(ur => ur.AspNetRole != null)
            .Select(ur => new EmployeeRoleResult
            {
                RoleID = ur.AspNetRole!.RoleID,
                RoleName = ur.AspNetRole.RoleName,
                NormalizedName = ur.AspNetRole.NormalizedName,
                ConcurrencyStamp = ur.AspNetRole.ConcurrencyStamp,
                RoleImage = ur.AspNetRole.RoleImage,
                IsVisible = ur.AspNetRole.IsVisible,
                IsDeleted = ur.AspNetRole.IsDeleted,
                IsDeletedBy = ur.AspNetRole.IsDeletedBy,
                IsDeletedWhy = ur.AspNetRole.IsDeletedWhy,
                CreatedBy = ur.AspNetRole.CreatedBy,
                DateIns = ur.AspNetRole.DateIns,
                EditedBy = ur.AspNetRole.EditedBy,
                DateEdit = ur.AspNetRole.DateEdit,
                RolePermissions = ur.AspNetRole.RolePermissions
                    .Select(p => new EmployeeRolePermissionResult
                    {
                        Id = p.Id,
                        RoleId = Guid.Empty,
                        Permission = p.Permission
                    })
                    .ToList()
            })
            .ToList();
    }

    private static List<EmployeeUserPermissionResult> MapAssignedPermissions(IEnumerable<AspNetUserPermission> permissions)
    {
        return permissions
            .Select(p => new EmployeeUserPermissionResult
            {
                Id = p.Id,
                UserId = p.UserId,
                Permission = p.Permission
            })
            .ToList();
    }

    private static List<EmployeeUserDeniedPermissionResult> MapDeniedPermissions(IEnumerable<AspNetUserDeniedPermission> permissions)
    {
        return permissions
            .Select(p => new EmployeeUserDeniedPermissionResult
            {
                Id = p.Id,
                UserId = p.UserId,
                Permission = p.Permission
            })
            .ToList();
    }
}
