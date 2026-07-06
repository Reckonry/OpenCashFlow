using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using global::Shared.Core;
using global::Shared.Data;
using global::Shared.DTOs;
using global::Shared.Models;
using global::Shared.Models.Cash;
using global::Shared.Models.Identity;

namespace OpenCashFlow.API.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class SetupController(ApplicationDbContext db, ILogger<SetupController> logger) : ControllerBase
    {
        [HttpGet("status")]
        public async Task<ActionResult<SetupStatus_DTO>> Status(CancellationToken cancellationToken)
        {
            return Ok(await GetStatusAsync(cancellationToken));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SetupRequest_DTO request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentStatus = await GetStatusAsync(cancellationToken);
            if (!currentStatus.RequiresSetup)
            {
                return Conflict(new { message = "This OpenCashFlow instance is already configured." });
            }

            if (currentStatus.HasCompanies || currentStatus.HasAdminUsers)
            {
                return Conflict(new { message = "Setup cannot continue because this instance is partially configured." });
            }

            if (!IsStrongPassword(request.AdminPassword))
            {
                return BadRequest(new { message = "Admin password must be at least 8 characters and include upper, lower, digit, and special characters." });
            }

            var normalizedEmail = request.AdminEmail.Trim();
            var now = DateTime.UtcNow;
            var tenantId = Guid.NewGuid();
            var adminUserId = Guid.NewGuid();
            var salt = PasswordHasher.GenerateSalt();

            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await EnsureSystemRolesAsync(cancellationToken);

                var company = new Company
                {
                    TenantID = tenantId,
                    CompanyName = request.CompanyName.Trim(),
                    MaxUsers = 1000,
                    StartingContract = now,
                    EndingContract = now.AddYears(100),
                    IsActive = true,
                    ContractAcepted = true,
                    ContractAcceptedDate = now,
                    ContractVersion = "self-hosted",
                    GdprConsent = true,
                    GdprConsentDate = now,
                    DefaultLanguage = request.Language.Trim().ToLowerInvariant(),
                    DefaultCurrency = request.Currency.Trim().ToUpperInvariant(),
                    DefaultTimezone = request.Timezone.Trim(),
                    DefaultCountry = request.Country.Trim().ToUpperInvariant(),
                    MasterPassword = null,
                    CompanySecret = Guid.NewGuid().ToString("N"),
                    CreatedBy = adminUserId,
                    DateIns = now
                };

                var admin = new AspNetUser
                {
                    UserID = adminUserId,
                    UserName = normalizedEmail,
                    Email = normalizedEmail,
                    EmailConfirmed = true,
                    IsApproved = true,
                    UserFirstName = request.AdminFirstName.Trim(),
                    UserLastName = string.IsNullOrWhiteSpace(request.AdminLastName) ? null : request.AdminLastName.Trim(),
                    Language = request.Language.Trim().ToUpperInvariant(),
                    Country = request.Country.Trim().ToUpperInvariant(),
                    Timezone = request.Timezone.Trim(),
                    PrivacyPolicyAcepted = true,
                    PrivacyPolicyAcceptedDate = now,
                    PasswordSalt = salt,
                    PasswordHash = PasswordHasher.HashPasswordArgon2(request.AdminPassword, salt),
                    DateIns = now
                };

                db.Company_DS.Add(company);
                db.AspNetUser_DS.Add(admin);
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
                return CreatedAtAction(nameof(Status), await GetStatusAsync(cancellationToken));
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private async Task<SetupStatus_DTO> GetStatusAsync(CancellationToken cancellationToken)
        {
            var hasCompanies = await db.Company_DS.AsNoTracking().AnyAsync(cancellationToken);
            var hasAdminUsers = await db.AspNetUserRole_DS.AsNoTracking()
                .AnyAsync(r => r.RoleID == Configuration.CompanyAdminRoleID || r.RoleID == Configuration.InstanceAdminRoleID, cancellationToken);

            return new SetupStatus_DTO
            {
                HasCompanies = hasCompanies,
                HasAdminUsers = hasAdminUsers,
                RequiresSetup = !hasCompanies || !hasAdminUsers
            };
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

        private static bool IsStrongPassword(string password)
        {
            return !string.IsNullOrWhiteSpace(password)
                && password.Length >= 8
                && password.Any(char.IsUpper)
                && password.Any(char.IsLower)
                && password.Any(char.IsDigit)
                && password.Any(ch => !char.IsLetterOrDigit(ch));
        }
    }
}
