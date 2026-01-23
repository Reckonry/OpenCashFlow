using OpenCashFlow.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using global::Shared.Data;
using global::Shared.DTOs.Admin;
using global::Shared.Models;
using global::Shared.Models.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenCashFlow.API.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserManagementService> _logger;
        private readonly IAuthenticationService _authenticationService;

        public UserManagementService(
            ApplicationDbContext context,
            ILogger<UserManagementService> logger,
            IAuthenticationService authenticationService)
        {
            _context = context;
            _logger = logger;
            _authenticationService = authenticationService;
        }

        public async Task<(List<User_List_DTO> Users, int TotalCount)> GetUsersAsync(
            User_Filter_DTO filter,
            CancellationToken cancellationToken = default)
        {
            var query = _context.AspNetUser_DS
                .Include(u => u.Roles).ThenInclude(ur => ur.AspNetRole)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var searchLower = filter.Search.ToLower();
                query = query.Where(u =>
                    u.UserName.ToLower().Contains(searchLower) ||
                    u.Email.ToLower().Contains(searchLower) ||
                    (u.UserFirstName != null && u.UserFirstName.ToLower().Contains(searchLower)) ||
                    (u.UserLastName != null && u.UserLastName.ToLower().Contains(searchLower))
                );
            }

            if (filter.IsActive.HasValue)
            {
                query = query.Where(u => u.IsApproved == filter.IsActive.Value);
            }

            if (filter.IsLocked.HasValue)
            {
                if (filter.IsLocked.Value)
                {
                    query = query.Where(u => u.LockoutEnabled && u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTime.UtcNow);
                }
                else
                {
                    query = query.Where(u => !u.LockoutEnabled || !u.LockoutEnd.HasValue || u.LockoutEnd.Value <= DateTime.UtcNow);
                }
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

            // Filter by company if specified
            if (filter.TenantID.HasValue)
            {
                var companyUserIds = await _context.Company_Staff_DS
                    .Where(cs => cs.TenantID == filter.TenantID.Value && !cs.IsDeleted)
                    .Select(cs => cs.UserID)
                    .ToListAsync(cancellationToken);

                query = query.Where(u => companyUserIds.Contains(u.UserID));
            }

            // Exclude soft-deleted users
            query = query.Where(u => !u.IsDeleted);

            // Count total before pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Sorting
            query = filter.SortBy switch
            {
                "Username" => filter.SortDescending ? query.OrderByDescending(u => u.UserName) : query.OrderBy(u => u.UserName),
                "Email" => filter.SortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                "LastLoginDate" => filter.SortDescending ? query.OrderByDescending(u => u.LastLoginDate) : query.OrderBy(u => u.LastLoginDate),
                _ => filter.SortDescending ? query.OrderByDescending(u => u.DateIns) : query.OrderBy(u => u.DateIns)
            };

            // Pagination
            var users = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            // Get company info for each user
            var userIds = users.Select(u => u.UserID).ToList();
            var companyStaffMap = await _context.Company_Staff_DS
                .Where(cs => userIds.Contains(cs.UserID) && !cs.IsDeleted)
                .ToDictionaryAsync(cs => cs.UserID, cs => cs, cancellationToken);

            var companyIds = companyStaffMap.Values.Select(cs => cs.TenantID).Distinct().ToList();
            var companyMap = await _context.Company_DS
                .Where(c => companyIds.Contains(c.TenantID) && !c.IsDeleted)
                .ToDictionaryAsync(c => c.TenantID, c => c, cancellationToken);

            // Map to DTOs
            var userDtos = users.Select(u => new User_List_DTO
            {
                UserID = u.UserID,
                Username = u.UserName,
                Email = u.Email,
                FirstName = u.UserFirstName,
                LastName = u.UserLastName,
                IsActive = u.IsApproved,
                IsLocked = u.LockoutEnabled && u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTime.UtcNow,
                LockoutEnd = u.LockoutEnd,
                CreatedDate = u.DateIns,
                LastLoginDate = u.LastLoginDate,
                Roles = u.Roles.Select(r => r.AspNetRole?.RoleName ?? string.Empty).ToArray(),
                AccessFailedCount = u.AccessFailedCount,
                EmailConfirmed = u.EmailConfirmed,
                CompanyName = companyStaffMap.ContainsKey(u.UserID) && companyMap.ContainsKey(companyStaffMap[u.UserID].TenantID)
                    ? companyMap[companyStaffMap[u.UserID].TenantID].CompanyName
                    : null,
                TenantID = companyStaffMap.ContainsKey(u.UserID) ? companyStaffMap[u.UserID].TenantID : null
            }).ToList();

            return (userDtos, totalCount);
        }

        public async Task<User_Detail_DTO> GetUserDetailAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _context.AspNetUser_DS
                .Include(u => u.Roles).ThenInclude(ur => ur.AspNetRole)
                .FirstOrDefaultAsync(u => u.UserID == userId && !u.IsDeleted, cancellationToken)
                ?? throw new KeyNotFoundException($"User {userId} not found.");

            // Get company info
            var companyStaff = await _context.Company_Staff_DS
                .FirstOrDefaultAsync(cs => cs.UserID == userId && !cs.IsDeleted, cancellationToken);

            Company? company = null;
            if (companyStaff != null)
            {
                company = await _context.Company_DS
                    .FirstOrDefaultAsync(c => c.TenantID == companyStaff.TenantID && !c.IsDeleted, cancellationToken);
            }

            // Build audit log
            var auditLog = new List<UserAuditEntry_DTO>();

            // User created
            auditLog.Add(new UserAuditEntry_DTO
            {
                Timestamp = user.DateIns,
                Action = "User Created",
                Details = $"User account created with username '{user.UserName}'",
                PerformedBy = user.CreatedBy?.ToString()
            });

            // Last edited
            if (user.DateEdit.HasValue)
            {
                auditLog.Add(new UserAuditEntry_DTO
                {
                    Timestamp = user.DateEdit.Value,
                    Action = "User Updated",
                    Details = "User information updated",
                    PerformedBy = user.EditedBy?.ToString()
                });
            }

            // Last login
            if (user.LastLoginDate.HasValue)
            {
                auditLog.Add(new UserAuditEntry_DTO
                {
                    Timestamp = user.LastLoginDate.Value,
                    Action = "User Login",
                    Details = "User logged in",
                    IPAddress = user.IpAddress
                });
            }

            // Lockout status
            if (user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            {
                auditLog.Add(new UserAuditEntry_DTO
                {
                    Timestamp = user.LockoutEnd.Value,
                    Action = "User Locked",
                    Details = $"User account locked until {user.LockoutEnd.Value:yyyy-MM-dd HH:mm:ss}"
                });
            }

            var dto = new User_Detail_DTO
            {
                UserID = user.UserID,
                Username = user.UserName,
                Email = user.Email,
                FirstName = user.UserFirstName,
                LastName = user.UserLastName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsApproved,
                IsLocked = user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow,
                LockoutEnd = user.LockoutEnd,
                CreatedDate = user.DateIns,
                LastLoginDate = user.LastLoginDate,
                Roles = user.Roles.Select(r => r.AspNetRole?.RoleName ?? string.Empty).ToArray(),
                AccessFailedCount = user.AccessFailedCount,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                CompanyName = company?.CompanyName,
                TenantID = companyStaff?.TenantID,
                AuditLog = auditLog.OrderByDescending(a => a.Timestamp).ToList()
            };

            return dto;
        }

        public async Task<User_Detail_DTO> CreateUserAsync(User_Create_DTO dto, CancellationToken cancellationToken = default)
        {
            // Validate unique username and email
            var existingUser = await _context.AspNetUser_DS
                .FirstOrDefaultAsync(u => u.UserName == dto.Username || u.Email == dto.Email, cancellationToken);

            if (existingUser != null)
            {
                throw new InvalidOperationException("Username or email already exists.");
            }

            // Verify company exists
            var company = await _context.Company_DS
                .FirstOrDefaultAsync(c => c.TenantID == dto.TenantID && !c.IsDeleted, cancellationToken)
                ?? throw new KeyNotFoundException($"Company {dto.TenantID} not found.");

            // Create user
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
                CreatedBy = _authenticationService.GetUserID()
            };

            // Hash password
            user.PasswordHash = HashPassword(dto.Password);
            user.PasswordSalt = GenerateSalt();

            _context.AspNetUser_DS.Add(user);

            // Create Company_Staff entry
            var companyStaff = new Company_Staff
            {
                UserID = user.UserID,
                TenantID = dto.TenantID,
                DateIns = DateTime.UtcNow,
                CreatedBy = _authenticationService.GetUserID()
            };

            _context.Company_Staff_DS.Add(companyStaff);

            // Add roles
            if (dto.Roles != null && dto.Roles.Length > 0)
            {
                foreach (var roleName in dto.Roles)
                {
                    var role = await _context.AspNetRole_DS.FirstOrDefaultAsync(r => r.RoleName == roleName, cancellationToken);
                    if (role != null)
                    {
                        _context.AspNetUserRole_DS.Add(new AspNetUserRole
                        {
                            UserID = user.UserID,
                            RoleID = role.RoleID
                        });
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {Username} created with ID {UserId} for company {CompanyId}",
                dto.Username, user.UserID, dto.TenantID);

            // TODO: Send welcome email if dto.SendWelcomeEmail is true

            return await GetUserDetailAsync(user.UserID, cancellationToken);
        }

        public async Task<User_Detail_DTO> UpdateUserAsync(User_Update_DTO dto, CancellationToken cancellationToken = default)
        {
            var user = await _context.AspNetUser_DS
                .FirstOrDefaultAsync(u => u.UserID == dto.UserID && !u.IsDeleted, cancellationToken)
                ?? throw new KeyNotFoundException($"User {dto.UserID} not found.");

            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                // Check email uniqueness
                var emailExists = await _context.AspNetUser_DS
                    .AnyAsync(u => u.Email == dto.Email && u.UserID != dto.UserID, cancellationToken);
                if (emailExists)
                {
                    throw new InvalidOperationException("Email already in use.");
                }
                user.Email = dto.Email;
                user.EmailConfirmed = false; // Require re-confirmation
            }

            if (dto.FirstName != null)
            {
                user.UserFirstName = dto.FirstName;
            }

            if (dto.LastName != null)
            {
                user.UserLastName = dto.LastName;
            }

            if (dto.PhoneNumber != null)
            {
                user.PhoneNumber = dto.PhoneNumber;
                user.PhoneNumberConfirmed = false; // Require re-confirmation
            }

            if (dto.IsActive.HasValue)
            {
                user.IsApproved = dto.IsActive.Value;
            }

            user.DateEdit = DateTime.UtcNow;
            user.EditedBy = _authenticationService.GetUserID();

            // Update company if provided
            if (dto.TenantID.HasValue)
            {
                var companyStaff = await _context.Company_Staff_DS
                    .FirstOrDefaultAsync(cs => cs.UserID == dto.UserID && !cs.IsDeleted, cancellationToken);

                if (companyStaff != null && companyStaff.TenantID != dto.TenantID.Value)
                {
                    companyStaff.TenantID = dto.TenantID.Value;
                    companyStaff.DateEdit = DateTime.UtcNow;
                    companyStaff.EditedBy = _authenticationService.GetUserID();
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {UserId} updated", dto.UserID);

            return await GetUserDetailAsync(dto.UserID, cancellationToken);
        }

        public async Task LockUserAsync(Guid userId, DateTime? lockoutEnd = null, CancellationToken cancellationToken = default)
        {
            var user = await _context.AspNetUser_DS
                .FirstOrDefaultAsync(u => u.UserID == userId && !u.IsDeleted, cancellationToken)
                ?? throw new KeyNotFoundException($"User {userId} not found.");

            user.LockoutEnabled = true;
            user.LockoutEnd = lockoutEnd ?? DateTime.UtcNow.AddYears(100); // Indefinite lock
            user.DateEdit = DateTime.UtcNow;
            user.EditedBy = _authenticationService.GetUserID();

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogWarning("User {UserId} locked until {LockoutEnd}", userId, user.LockoutEnd);
        }

        public async Task UnlockUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _context.AspNetUser_DS
                .FirstOrDefaultAsync(u => u.UserID == userId && !u.IsDeleted, cancellationToken)
                ?? throw new KeyNotFoundException($"User {userId} not found.");

            user.LockoutEnabled = false;
            user.LockoutEnd = null;
            user.AccessFailedCount = 0;
            user.DateEdit = DateTime.UtcNow;
            user.EditedBy = _authenticationService.GetUserID();

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {UserId} unlocked", userId);
        }

        public async Task ResetPasswordAsync(User_ResetPassword_DTO dto, CancellationToken cancellationToken = default)
        {
            var user = await _context.AspNetUser_DS
                .FirstOrDefaultAsync(u => u.UserID == dto.UserID && !u.IsDeleted, cancellationToken)
                ?? throw new KeyNotFoundException($"User {dto.UserID} not found.");

            user.PasswordHash = HashPassword(dto.NewPassword);
            user.PasswordSalt = GenerateSalt();
            user.UserMustChangePassword = dto.RequirePasswordChange;
            user.DateEdit = DateTime.UtcNow;
            user.EditedBy = _authenticationService.GetUserID();

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Password reset for user {UserId}", dto.UserID);

            // TODO: Send notification email if dto.SendNotificationEmail is true
        }

        public async Task UpdateUserRolesAsync(User_Roles_DTO dto, CancellationToken cancellationToken = default)
        {
            var user = await _context.AspNetUser_DS
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.UserID == dto.UserID && !u.IsDeleted, cancellationToken)
                ?? throw new KeyNotFoundException($"User {dto.UserID} not found.");

            // Remove existing roles
            var existingRoles = await _context.AspNetUserRole_DS
                .Where(ur => ur.UserID == dto.UserID)
                .ToListAsync(cancellationToken);

            _context.AspNetUserRole_DS.RemoveRange(existingRoles);

            // Add new roles
            foreach (var roleName in dto.Roles)
            {
                var role = await _context.AspNetRole_DS
                    .FirstOrDefaultAsync(r => r.RoleName == roleName, cancellationToken);

                if (role != null)
                {
                    _context.AspNetUserRole_DS.Add(new AspNetUserRole
                    {
                        UserID = dto.UserID,
                        RoleID = role.RoleID
                    });
                }
            }

            user.DateEdit = DateTime.UtcNow;
            user.EditedBy = _authenticationService.GetUserID();

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Roles updated for user {UserId}: {Roles}", dto.UserID, string.Join(", ", dto.Roles));
        }

        public async Task<List<Role_DTO>> GetRolesAsync(CancellationToken cancellationToken = default)
        {
            var roles = await _context.AspNetRole_DS
                .Select(r => new
                {
                    r.RoleName,
                    UserCount = _context.AspNetUserRole_DS.Count(ur => ur.RoleID == r.RoleID)
                })
                .ToListAsync(cancellationToken);

            return roles.Select(r => new Role_DTO
            {
                RoleName = r.RoleName ?? string.Empty,
                Description = string.Empty,
                UserCount = r.UserCount
            }).ToList();
        }

        public async Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _context.AspNetUser_DS
                .FirstOrDefaultAsync(u => u.UserID == userId && !u.IsDeleted, cancellationToken)
                ?? throw new KeyNotFoundException($"User {userId} not found.");

            // Soft delete
            user.IsDeleted = true;
            user.IsDeletedBy = _authenticationService.GetUserID();
            user.IsDeletedWhy = "Deleted by admin";

            // Also soft delete company staff entry
            var companyStaff = await _context.Company_Staff_DS
                .FirstOrDefaultAsync(cs => cs.UserID == userId && !cs.IsDeleted, cancellationToken);

            if (companyStaff != null)
            {
                companyStaff.IsDeleted = true;
                companyStaff.IsDeletedBy = _authenticationService.GetUserID();
                companyStaff.IsDeletedWhy = "User deleted by admin";
                companyStaff.DateDeleted = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogWarning("User {UserId} deleted", userId);
        }

        #region Helper Methods

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

        #endregion
    }
}
