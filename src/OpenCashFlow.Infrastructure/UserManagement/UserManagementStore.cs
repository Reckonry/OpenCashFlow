using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenCashFlow.Application.UserManagement.Models;
using OpenCashFlow.Application.UserManagement.Ports;
using OpenCashFlow.Infrastructure.Persistence;
using OpenCashFlow.Infrastructure.Persistence.Entities;
using OpenCashFlow.Infrastructure.Persistence.Entities.Identity;

namespace OpenCashFlow.Infrastructure.UserManagement;

public sealed class UserManagementStore(
    ApplicationDbContext context,
    ILogger<UserManagementStore> logger) : IUserManagementStore
{
    public async Task<(IReadOnlyList<AdminUserListItem> Users, int TotalCount)> GetUsersAsync(
        AdminUserFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = context.AspNetUser_DS
            .Include(u => u.Roles).ThenInclude(ur => ur.AspNetRole)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchLower = filter.Search.ToLower();
            query = query.Where(u =>
                u.UserName.ToLower().Contains(searchLower) ||
                u.Email.ToLower().Contains(searchLower) ||
                (u.UserFirstName != null && u.UserFirstName.ToLower().Contains(searchLower)) ||
                (u.UserLastName != null && u.UserLastName.ToLower().Contains(searchLower)));
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(u => u.IsApproved == filter.IsActive.Value);
        }

        if (filter.IsLocked.HasValue)
        {
            query = filter.IsLocked.Value
                ? query.Where(u => u.LockoutEnabled && u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTime.UtcNow)
                : query.Where(u => !u.LockoutEnabled || !u.LockoutEnd.HasValue || u.LockoutEnd.Value <= DateTime.UtcNow);
        }

        if (filter.EmailConfirmed.HasValue)
        {
            query = query.Where(u => u.EmailConfirmed == filter.EmailConfirmed.Value);
        }

        if (filter.TwoFactorEnabled.HasValue)
        {
            query = query.Where(u => u.TwoFactorEnabled == filter.TwoFactorEnabled.Value);
        }

        if (filter.CreatedFrom.HasValue)
        {
            query = query.Where(u => u.DateIns >= filter.CreatedFrom.Value);
        }

        if (filter.CreatedTo.HasValue)
        {
            query = query.Where(u => u.DateIns <= filter.CreatedTo.Value);
        }

        if (filter.LastLoginFrom.HasValue)
        {
            query = query.Where(u => u.LastLoginDate.HasValue && u.LastLoginDate.Value >= filter.LastLoginFrom.Value);
        }

        if (filter.LastLoginTo.HasValue)
        {
            query = query.Where(u => u.LastLoginDate.HasValue && u.LastLoginDate.Value <= filter.LastLoginTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Role))
        {
            query = query.Where(u => u.Roles.Any(r => r.AspNetRole != null && r.AspNetRole.RoleName == filter.Role));
        }

        if (filter.TenantId.HasValue)
        {
            var companyUserIds = await context.Company_Staff_DS
                .Where(cs => cs.TenantID == filter.TenantId.Value && !cs.IsDeleted)
                .Select(cs => cs.UserID)
                .ToListAsync(cancellationToken);

            query = query.Where(u => companyUserIds.Contains(u.UserID));
        }

        query = query.Where(u => !u.IsDeleted);

        var totalCount = await query.CountAsync(cancellationToken);

        query = filter.SortBy switch
        {
            "Username" => filter.SortDescending ? query.OrderByDescending(u => u.UserName) : query.OrderBy(u => u.UserName),
            "Email" => filter.SortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            "LastLoginDate" => filter.SortDescending ? query.OrderByDescending(u => u.LastLoginDate) : query.OrderBy(u => u.LastLoginDate),
            _ => filter.SortDescending ? query.OrderByDescending(u => u.DateIns) : query.OrderBy(u => u.DateIns)
        };

        var users = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        var userIds = users.Select(u => u.UserID).ToList();
        var companyStaffMap = await context.Company_Staff_DS
            .Where(cs => userIds.Contains(cs.UserID) && !cs.IsDeleted)
            .ToDictionaryAsync(cs => cs.UserID, cs => cs, cancellationToken);

        var companyIds = companyStaffMap.Values.Select(cs => cs.TenantID).Distinct().ToList();
        var companyMap = await context.Company_DS
            .Where(c => companyIds.Contains(c.TenantID) && !c.IsDeleted)
            .ToDictionaryAsync(c => c.TenantID, c => c, cancellationToken);

        var results = users.Select(u => new AdminUserListItem(
            u.UserID,
            u.UserName,
            u.Email,
            u.UserFirstName,
            u.UserLastName,
            u.IsApproved,
            u.LockoutEnabled && u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTime.UtcNow,
            u.LockoutEnd,
            u.DateIns,
            u.LastLoginDate,
            u.Roles.Select(r => r.AspNetRole?.RoleName ?? string.Empty).ToArray(),
            companyStaffMap.ContainsKey(u.UserID) && companyMap.ContainsKey(companyStaffMap[u.UserID].TenantID)
                ? companyMap[companyStaffMap[u.UserID].TenantID].CompanyName
                : null,
            companyStaffMap.ContainsKey(u.UserID) ? companyStaffMap[u.UserID].TenantID : null,
            u.AccessFailedCount,
            u.EmailConfirmed)).ToList();

        return (results, totalCount);
    }

    public async Task<AdminUserDetail> GetUserDetailAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await context.AspNetUser_DS
            .Include(u => u.Roles).ThenInclude(ur => ur.AspNetRole)
            .FirstOrDefaultAsync(u => u.UserID == userId && !u.IsDeleted, cancellationToken)
            ?? throw new KeyNotFoundException($"User {userId} not found.");

        var companyStaff = await context.Company_Staff_DS
            .FirstOrDefaultAsync(cs => cs.UserID == userId && !cs.IsDeleted, cancellationToken);

        Company? company = null;
        if (companyStaff != null)
        {
            company = await context.Company_DS
                .FirstOrDefaultAsync(c => c.TenantID == companyStaff.TenantID && !c.IsDeleted, cancellationToken);
        }

        return MapDetail(user, companyStaff, company);
    }

    public async Task<AdminUserDetail> CreateUserAsync(AdminUserCreate dto, CancellationToken cancellationToken = default)
    {
        var existingUser = await context.AspNetUser_DS
            .FirstOrDefaultAsync(u => u.UserName == dto.Username || u.Email == dto.Email, cancellationToken);

        if (existingUser != null)
        {
            throw new InvalidOperationException("Username or email already exists.");
        }

        _ = await context.Company_DS
            .FirstOrDefaultAsync(c => c.TenantID == dto.TenantId && !c.IsDeleted, cancellationToken)
            ?? throw new KeyNotFoundException($"Company {dto.TenantId} not found.");

        var user = new AspNetUser
        {
            UserID = Guid.NewGuid(),
            UserName = dto.Username,
            Email = dto.Email,
            UserFirstName = dto.FirstName ?? string.Empty,
            UserLastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber,
            IsApproved = true,
            EmailConfirmed = false,
            UserMustChangePassword = dto.RequirePasswordChange,
            DateIns = DateTime.UtcNow,
            CreatedBy = dto.CurrentUserId,
            PasswordHash = HashPassword(dto.Password),
            PasswordSalt = GenerateSalt()
        };

        context.AspNetUser_DS.Add(user);

        context.Company_Staff_DS.Add(new Company_Staff
        {
            UserID = user.UserID,
            TenantID = dto.TenantId,
            DateIns = DateTime.UtcNow,
            CreatedBy = dto.CurrentUserId
        });

        foreach (var roleName in dto.Roles)
        {
            var role = await context.AspNetRole_DS.FirstOrDefaultAsync(r => r.RoleName == roleName, cancellationToken);
            if (role != null)
            {
                context.AspNetUserRole_DS.Add(new AspNetUserRole
                {
                    UserID = user.UserID,
                    RoleID = role.RoleID
                });
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User {Username} created with ID {UserId} for company {CompanyId}",
            dto.Username, user.UserID, dto.TenantId);

        return await GetUserDetailAsync(user.UserID, cancellationToken);
    }

    public async Task<AdminUserDetail> UpdateUserAsync(AdminUserUpdate dto, CancellationToken cancellationToken = default)
    {
        var user = await context.AspNetUser_DS
            .FirstOrDefaultAsync(u => u.UserID == dto.UserId && !u.IsDeleted, cancellationToken)
            ?? throw new KeyNotFoundException($"User {dto.UserId} not found.");

        if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
        {
            var emailExists = await context.AspNetUser_DS
                .AnyAsync(u => u.Email == dto.Email && u.UserID != dto.UserId, cancellationToken);
            if (emailExists)
            {
                throw new InvalidOperationException("Email already in use.");
            }

            user.Email = dto.Email;
            user.EmailConfirmed = false;
        }

        if (dto.FirstName != null) user.UserFirstName = dto.FirstName;
        if (dto.LastName != null) user.UserLastName = dto.LastName;
        if (dto.PhoneNumber != null)
        {
            user.PhoneNumber = dto.PhoneNumber;
            user.PhoneNumberConfirmed = false;
        }

        if (dto.IsActive.HasValue) user.IsApproved = dto.IsActive.Value;

        user.DateEdit = DateTime.UtcNow;
        user.EditedBy = dto.CurrentUserId;

        if (dto.TenantId.HasValue)
        {
            var companyStaff = await context.Company_Staff_DS
                .FirstOrDefaultAsync(cs => cs.UserID == dto.UserId && !cs.IsDeleted, cancellationToken);

            if (companyStaff != null && companyStaff.TenantID != dto.TenantId.Value)
            {
                companyStaff.TenantID = dto.TenantId.Value;
                companyStaff.DateEdit = DateTime.UtcNow;
                companyStaff.EditedBy = dto.CurrentUserId;
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User {UserId} updated", dto.UserId);

        return await GetUserDetailAsync(dto.UserId, cancellationToken);
    }

    public async Task LockUserAsync(Guid userId, DateTime? lockoutEnd, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var user = await GetTrackedUserAsync(userId, cancellationToken);
        user.LockoutEnabled = true;
        user.LockoutEnd = lockoutEnd ?? DateTime.UtcNow.AddYears(100);
        user.DateEdit = DateTime.UtcNow;
        user.EditedBy = currentUserId;

        await context.SaveChangesAsync(cancellationToken);
        logger.LogWarning("User {UserId} locked until {LockoutEnd}", userId, user.LockoutEnd);
    }

    public async Task UnlockUserAsync(Guid userId, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var user = await GetTrackedUserAsync(userId, cancellationToken);
        user.LockoutEnabled = false;
        user.LockoutEnd = null;
        user.AccessFailedCount = 0;
        user.DateEdit = DateTime.UtcNow;
        user.EditedBy = currentUserId;

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("User {UserId} unlocked", userId);
    }

    public async Task ResetPasswordAsync(AdminUserResetPassword dto, CancellationToken cancellationToken = default)
    {
        var user = await GetTrackedUserAsync(dto.UserId, cancellationToken);
        user.PasswordHash = HashPassword(dto.NewPassword);
        user.PasswordSalt = GenerateSalt();
        user.UserMustChangePassword = dto.RequirePasswordChange;
        user.DateEdit = DateTime.UtcNow;
        user.EditedBy = dto.CurrentUserId;

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Password reset for user {UserId}", dto.UserId);
    }

    public async Task UpdateUserRolesAsync(Guid userId, string[] roles, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var user = await context.AspNetUser_DS
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.UserID == userId && !u.IsDeleted, cancellationToken)
            ?? throw new KeyNotFoundException($"User {userId} not found.");

        var existingRoles = await context.AspNetUserRole_DS
            .Where(ur => ur.UserID == userId)
            .ToListAsync(cancellationToken);

        context.AspNetUserRole_DS.RemoveRange(existingRoles);

        foreach (var roleName in roles)
        {
            var role = await context.AspNetRole_DS
                .FirstOrDefaultAsync(r => r.RoleName == roleName, cancellationToken);

            if (role != null)
            {
                context.AspNetUserRole_DS.Add(new AspNetUserRole
                {
                    UserID = userId,
                    RoleID = role.RoleID
                });
            }
        }

        user.DateEdit = DateTime.UtcNow;
        user.EditedBy = currentUserId;

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Roles updated for user {UserId}: {Roles}", userId, string.Join(", ", roles));
    }

    public async Task<IReadOnlyList<AdminRoleSummary>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await context.AspNetRole_DS
            .Select(r => new
            {
                r.RoleName,
                UserCount = context.AspNetUserRole_DS.Count(ur => ur.RoleID == r.RoleID)
            })
            .ToListAsync(cancellationToken);

        return roles
            .Select(r => new AdminRoleSummary(r.RoleName ?? string.Empty, string.Empty, r.UserCount))
            .ToList();
    }

    public async Task DeleteUserAsync(Guid userId, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var user = await GetTrackedUserAsync(userId, cancellationToken);
        user.IsDeleted = true;
        user.IsDeletedBy = currentUserId;
        user.IsDeletedWhy = "Deleted by admin";

        var companyStaff = await context.Company_Staff_DS
            .FirstOrDefaultAsync(cs => cs.UserID == userId && !cs.IsDeleted, cancellationToken);

        if (companyStaff != null)
        {
            companyStaff.IsDeleted = true;
            companyStaff.IsDeletedBy = currentUserId;
            companyStaff.IsDeletedWhy = "User deleted by admin";
            companyStaff.DateDeleted = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);
        logger.LogWarning("User {UserId} deleted", userId);
    }

    private async Task<AspNetUser> GetTrackedUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await context.AspNetUser_DS
            .FirstOrDefaultAsync(u => u.UserID == userId && !u.IsDeleted, cancellationToken)
            ?? throw new KeyNotFoundException($"User {userId} not found.");
    }

    private static AdminUserDetail MapDetail(AspNetUser user, Company_Staff? companyStaff, Company? company)
    {
        var auditLog = new List<AdminUserAuditEntry>
        {
            new(
                user.DateIns,
                "User Created",
                $"User account created with username '{user.UserName}'",
                user.CreatedBy?.ToString(),
                null)
        };

        if (user.DateEdit.HasValue)
        {
            auditLog.Add(new AdminUserAuditEntry(
                user.DateEdit.Value,
                "User Updated",
                "User information updated",
                user.EditedBy?.ToString(),
                null));
        }

        if (user.LastLoginDate.HasValue)
        {
            auditLog.Add(new AdminUserAuditEntry(
                user.LastLoginDate.Value,
                "User Login",
                "User logged in",
                null,
                user.IpAddress));
        }

        if (user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
        {
            auditLog.Add(new AdminUserAuditEntry(
                user.LockoutEnd.Value,
                "User Locked",
                $"User account locked until {user.LockoutEnd.Value:yyyy-MM-dd HH:mm:ss}",
                null,
                null));
        }

        return new AdminUserDetail(
            user.UserID,
            user.UserName,
            user.Email,
            user.UserFirstName,
            user.UserLastName,
            user.PhoneNumber,
            user.IsApproved,
            user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow,
            user.LockoutEnd,
            user.DateIns,
            user.LastLoginDate,
            user.Roles.Select(r => r.AspNetRole?.RoleName ?? string.Empty).ToArray(),
            companyStaff?.TenantID,
            company?.CompanyName,
            user.AccessFailedCount,
            user.EmailConfirmed,
            user.PhoneNumberConfirmed,
            user.TwoFactorEnabled,
            auditLog.OrderByDescending(a => a.Timestamp).ToList());
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private static string GenerateSalt()
    {
        var saltBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }
}

