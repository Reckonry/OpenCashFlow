using AutoMapper;
using OpenCashFlow.API.Repositories.Interfaces;
using OpenCashFlow.Shared.Mappings;
using Microsoft.EntityFrameworkCore;
using global::Shared.Core;
using global::Shared.Data;
using global::Shared.DTOs.Employees;
using global::Shared.Models;
using global::Shared.Models.Identity;

namespace OpenCashFlow.API.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EmployeeRepository> _logger;
        private readonly IMapper _mapper;

        public EmployeeRepository(ApplicationDbContext context, ILogger<EmployeeRepository> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Company_Staff>?> GetEmployeesAsync(Guid TenantID, CancellationToken cancellationToken)
        {
            try
            {
                return await _context.Company_Staff_DS
                    .AsNoTracking()
                    .Where(e => e.TenantID == TenantID && !e.IsDeleted)
                    .Include(e => e.User)
                        .ThenInclude(u => u!.Roles)
                            .ThenInclude(ur => ur.AspNetRole)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employees for company {TenantID}", TenantID);
                return null;
            }
        }

        public async Task<Company_Staff?> GetEmployeeByIdAsync(Guid UserID, Guid TenantID, CancellationToken cancellationToken)
        {
            try
            {
                return await _context.Company_Staff_DS
                    .AsNoTracking()
                    .Include(e => e.User)
                        .ThenInclude(u => u!.Roles)
                            .ThenInclude(ur => ur.AspNetRole)
                    .FirstOrDefaultAsync(e => e.UserID == UserID && e.TenantID == TenantID && !e.IsDeleted, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee with ID {UserID}", UserID);
                return null;
            }
        }
        public async Task<AspNetUser?> GetUserAsync(string userInput, CancellationToken cancellationToken)
        {
            return await _context.AspNetUser_DS
                .AsNoTracking()
                .Where(p => p.UserName == userInput || p.Email == userInput || p.PhoneNumber == userInput)
                .FirstOrDefaultAsync(cancellationToken);
        }

        //public async Task CreateEmployeeAsync(Employee_Create_DTO employee, CancellationToken cancellationToken)
        //{
        //    //var config = new MapperConfiguration(cfg =>
        //    //{
        //    //    cfg.AddProfile<MappingProfile>();
        //    //});

        //    //try
        //    //{
        //    //    config.AssertConfigurationIsValid(); // <-- questa linea ti dice esattamente DOVE fallisce
        //    //}
        //    //catch (AutoMapperConfigurationException ex)
        //    //{
        //    //    Console.WriteLine(ex.Message);
        //    //    throw; // o loggalo meglio se sei in contesto web
        //    //}

        //    var AspNetUserEntity = _mapper.Map<AspNetUser>(employee);
        //    await _context.AspNetUser_DS.AddAsync(AspNetUserEntity, cancellationToken);

        //    var companyStaffEntity = _mapper.Map<Company_Staff>(employee);
        //    await _context.Company_Staff_DS.AddAsync(companyStaffEntity, cancellationToken);

        //    var AspNetUserRole = _mapper.Map<AspNetUserRole>(employee);
        //    await _context.AspNetUserRole_DS.AddAsync(AspNetUserRole, cancellationToken);

        //    await _context.SaveChangesAsync(cancellationToken);
        //}

        public async Task UpdateEmployeeAsync(AspNetUser employee, CancellationToken cancellationToken)
        {
            _context.AspNetUser_DS.Update(employee);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteEmployeeAsync(AspNetUser employee, CancellationToken cancellationToken)
        {
            try
            {
                // Soft delete with data anonymization
                employee.IsDeleted = true;
                // FREE THE EMAIL AND USERNAME: Set to anonymized values to make originals available for reuse
                employee.Email = $"deleted.{employee.UserID:N}@anonymized.local"; // Email becomes available again
                employee.UserName = $"deleted.{employee.UserID:N}"; // Username becomes available again
                employee.UserFirstName = "Utente";
                employee.UserLastName = "Eliminato";
                employee.PhoneNumber = null;
                employee.PhoneNumberPrefix = null;
                employee.PasswordHash = "DELETED_ACCOUNT"; // Cannot be null due to constraint
                employee.PasswordSalt = "DELETED_ACCOUNT"; // Cannot be null due to constraint
                employee.QuickLoginPinHash = null;
                employee.PasswordResetToken = null;
                employee.PasswordResetTokenValidUntil = null;

                _context.AspNetUser_DS.Update(employee);

                // Also mark Company_Staff as deleted
                var companyStaff = await _context.Company_Staff_DS
                    .FirstOrDefaultAsync(cs => cs.UserID == employee.UserID, cancellationToken);
                if (companyStaff != null)
                {
                    companyStaff.IsDeleted = true;
                    _context.Company_Staff_DS.Update(companyStaff);
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting employee with ID {UserID}", employee.UserID);
                throw new InvalidOperationException("Errore durante l'eliminazione del dipendente");
            }
        }

        public async Task CreateEmployeeAsync(Employee_Create_DTO employee, CancellationToken cancellationToken)
        {
            //var config = new MapperConfiguration(cfg =>
            //{
            //    cfg.AddProfile<MappingProfile>();
            //});

            //try
            //{
            //    config.AssertConfigurationIsValid(); // <-- questa linea ti dice esattamente DOVE fallisce
            //}
            //catch (AutoMapperConfigurationException ex)
            //{
            //    Console.WriteLine(ex.Message);
            //    throw; // o loggalo meglio se sei in contesto web
            //}

            var AspNetUserEntity = _mapper.Map<AspNetUser>(employee);
            var salt = PasswordHasher.GenerateSalt();
            AspNetUserEntity.PasswordSalt = salt;
            AspNetUserEntity.PasswordHash = PasswordHasher.HashPasswordArgon2(employee.TmpNewPassword ?? string.Empty, salt);
            AspNetUserEntity.QuickLoginPinHash = PasswordHasher.HashPasswordArgon2(employee.TmpFastLoginPin ?? string.Empty, salt);
            AspNetUserEntity.UserMustChangePassword = true; // forza cambio password al primo accesso
            await _context.AspNetUser_DS.AddAsync(AspNetUserEntity, cancellationToken);

            var companyStaffEntity = _mapper.Map<Company_Staff>(employee);
            await _context.Company_Staff_DS.AddAsync(companyStaffEntity, cancellationToken);

            // Roles are not assigned from creation DTO anymore; manage user roles separately.

            await _context.SaveChangesAsync(cancellationToken);

            // Assign selected role if provided (single-role at creation)
            if (employee.SelectedRoleID.HasValue)
            {
                var roleId = employee.SelectedRoleID.Value;
                var exists = await _context.AspNetUserRole_DS
                    .AnyAsync(ur => ur.UserID == AspNetUserEntity.UserID && ur.RoleID == roleId, cancellationToken);
                if (!exists)
                {
                    await _context.AspNetUserRole_DS.AddAsync(new AspNetUserRole
                    {
                        UserID = AspNetUserEntity.UserID,
                        RoleID = roleId
                    }, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
        }

        public async Task<bool> IsFastLoginPinInUseAsync(Guid TenantID, string pin, CancellationToken cancellationToken)
        {
            // Recupera tutti i salt e hash dei PIN degli utenti attivi dell'azienda
            var userPins = await _context.Company_Staff_DS
                .AsNoTracking()
                .Where(cs => cs.TenantID == TenantID && !cs.IsDeleted && cs.User != null && !string.IsNullOrWhiteSpace(cs.User.PasswordSalt))
                .Select(cs => new { cs.User!.PasswordSalt, cs.User.QuickLoginPinHash })
                .ToListAsync(cancellationToken);

            // Controlla lato client se il PIN hashato corrisponde
            foreach (var user in userPins)
            {
                var hashedPin = PasswordHasher.HashPasswordArgon2(pin, user.PasswordSalt);
                if (hashedPin == user.QuickLoginPinHash)
                    return true;
            }

            return false;
        }

        public async Task UpdateEmployeeAsync(Employee_Update_DTO employee, CancellationToken cancellationToken)
        {
            // Get the existing user from database to preserve password-related fields
            var existingUser = await _context.AspNetUser_DS.FirstOrDefaultAsync(u => u.UserID == employee.UserID, cancellationToken);
            if (existingUser == null)
                throw new InvalidOperationException("User not found");

            // Map the update data to the existing entity (preserving password fields)
            _mapper.Map(employee, existingUser);

            // Explicitly preserve password-related fields that should not be modified during profile update
            var entry = _context.Entry(existingUser);
            entry.Property(x => x.PasswordHash).IsModified = false;
            entry.Property(x => x.PasswordSalt).IsModified = false;
            entry.Property(x => x.QuickLoginPinHash).IsModified = false;

            var companyStaffEntity = _mapper.Map<Company_Staff>(employee);
            _context.Company_Staff_DS.Update(companyStaffEntity);

            // Single-role replace for visible roles
            if (employee.SelectedRoleID.HasValue)
            {
                var userId = employee.UserID;
                var selected = employee.SelectedRoleID.Value;

                // Remove all visible roles except the selected one
                var visibleAssignments = await _context.AspNetUserRole_DS
                    .Include(ur => ur.AspNetRole)
                    .Where(ur => ur.UserID == userId && ur.AspNetRole != null && ur.AspNetRole.IsVisible)
                    .ToListAsync(cancellationToken);

                var toRemove = visibleAssignments.Where(ur => ur.RoleID != selected).ToList();
                if (toRemove.Any())
                    _context.AspNetUserRole_DS.RemoveRange(toRemove);

                // Ensure the selected role is assigned
                var hasSelected = visibleAssignments.Any(ur => ur.RoleID == selected) ||
                    await _context.AspNetUserRole_DS.AnyAsync(ur => ur.UserID == userId && ur.RoleID == selected, cancellationToken);
                if (!hasSelected)
                {
                    await _context.AspNetUserRole_DS.AddAsync(new AspNetUserRole { UserID = userId, RoleID = selected }, cancellationToken);
                }
            }
            // Legacy multi-role sync (if provided explicitly)
            else if (employee.RoleIDs != null)
            {
                var userId = employee.UserID;
                var existing = await _context.AspNetUserRole_DS
                    .Where(r => r.UserID == userId)
                    .ToListAsync(cancellationToken);

                var existingRoleIds = existing.Select(e => e.RoleID).ToHashSet();
                var desiredRoleIds = employee.RoleIDs.ToHashSet();

                // To add
                var toAdd = desiredRoleIds.Except(existingRoleIds)
                    .Select(rid => new AspNetUserRole { UserID = userId, RoleID = rid });
                if (toAdd.Any())
                    await _context.AspNetUserRole_DS.AddRangeAsync(toAdd, cancellationToken);

                // To remove
                var toRemove = existing.Where(e => !desiredRoleIds.Contains(e.RoleID)).ToList();
                if (toRemove.Any())
                    _context.AspNetUserRole_DS.RemoveRange(toRemove);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateMyProfileAsync(Guid UserID, Employee_MyProfile_Update_DTO model, CancellationToken cancellationToken)
        {
            var user = await _context.AspNetUser_DS.FirstOrDefaultAsync(u => u.UserID == UserID, cancellationToken);
            if (user == null) return;

            if (!string.IsNullOrWhiteSpace(model.UserFirstName)) user.UserFirstName = model.UserFirstName;
            user.UserLastName = model.UserLastName;
            if (!string.IsNullOrWhiteSpace(model.Email)) user.Email = model.Email;
            user.PhoneNumberPrefix = model.PhoneNumberPrefix;
            user.PhoneNumber = model.PhoneNumber;
            if (!string.IsNullOrWhiteSpace(model.Language)) user.Language = model.Language;
            if (!string.IsNullOrWhiteSpace(model.Country)) user.Country = model.Country;
            user.Timezone = model.Timezone;

            _context.AspNetUser_DS.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
        }


        public async Task CreateResetTokenAsync(Guid UserID, string token, DateTime expiresAt, CancellationToken cancellationToken)
        {
            var user = await _context.AspNetUser_DS
                .FirstOrDefaultAsync(f => f.UserID == UserID, cancellationToken);

            if (user is null)
                throw new InvalidOperationException("Utente non trovato");

            user.PasswordResetToken = token;
            user.PasswordResetTokenValidUntil = expiresAt;

            _context.AspNetUser_DS.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
        }
        public async Task<bool> HasValidTokenAsync(Guid UserID, string token, CancellationToken cancellationToken)
        {
            return await _context.AspNetUser_DS
                .AsNoTracking()
                .Where(t => t.PasswordResetToken == token && t.PasswordResetTokenValidUntil > DateTime.UtcNow && t.UserID == UserID)
                .AnyAsync(cancellationToken);
        }

        public async Task UpdatePasswordAsync(Guid UserID, string newPassword, CancellationToken cancellationToken)
        {
            var user = await _context.AspNetUser_DS.FirstOrDefaultAsync(f => f.UserID == UserID, cancellationToken);
            if (user is null)
                throw new InvalidOperationException("Utente non trovato");

            user.PasswordHash = PasswordHasher.HashPasswordArgon2(newPassword, user.PasswordSalt);
            user.PasswordResetToken = null; // Clear the reset token after successful password change
            user.PasswordResetTokenValidUntil = null; // Clear the token validity date
            user.UserMustChangePassword = false; // rimuove l'obbligo dopo il cambio
            _context.AspNetUser_DS.Update(user);
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Error updating password for user {UserID}", UserID);
                throw new Exception("Unable to update password due to concurrency issues. Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating password for user {UserID}", UserID);
                throw new Exception("An error occurred while updating the password. Please try again.");
            }
        }

        public async Task RemovePasswordChangeRequirementAsync(Guid UserID, CancellationToken cancellationToken)
        {
            var user = await _context.AspNetUser_DS.FirstOrDefaultAsync(f => f.UserID == UserID, cancellationToken);
            if (user is null)
                throw new InvalidOperationException("Utente non trovato");

            user.UserMustChangePassword = false;
            _context.AspNetUser_DS.Update(user);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing password change requirement for user {UserID}", UserID);
                throw new Exception("An error occurred while removing password change requirement. Please try again.");
            }
        }

        public async Task<Guid?> GetUserIdFromResetTokenAsync(string token, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _context.AspNetUser_DS
                    .AsNoTracking()
                    .Where(u => u.PasswordResetToken == token && u.PasswordResetTokenValidUntil > DateTime.UtcNow)
                    .FirstOrDefaultAsync(cancellationToken);

                return user?.UserID;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user ID from reset token");
                return null;
            }
        }

        public async Task InvalidateResetTokenAsync(Guid UserID, string token, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _context.AspNetUser_DS
                    .Where(u => u.UserID == UserID && u.PasswordResetToken == token)
                    .FirstOrDefaultAsync(cancellationToken);

                if (user != null)
                {
                    user.PasswordResetToken = null;
                    user.PasswordResetTokenValidUntil = null;
                    _context.AspNetUser_DS.Update(user);
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating reset token for user {UserID}", UserID);
                throw new Exception("An error occurred while invalidating the reset token.");
            }
        }

        public async Task<AspNetUser?> GetUserByIdAsync(Guid UserID, CancellationToken cancellationToken)
        {
            try
            {
                return await _context.AspNetUser_DS
                    .AsNoTracking()
                    .Where(u => u.UserID == UserID)
                    .FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by ID {UserID}", UserID);
                return null;
            }
        }

        public async Task<(bool IsValid, bool IsExpired, AspNetUser? User)> ValidateResetTokenAsync(string token, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return (false, false, null);

                var user = await _context.AspNetUser_DS
                    .AsNoTracking()
                    .Where(u => u.PasswordResetToken == token)
                    .FirstOrDefaultAsync(cancellationToken);

                if (user == null)
                    return (false, false, null);

                // Controlla se il token è scaduto
                if (user.PasswordResetTokenValidUntil == null || user.PasswordResetTokenValidUntil <= DateTime.UtcNow)
                    return (false, true, user);

                return (true, false, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating reset token");
                return (false, false, null);
            }
        }

        public async Task<bool> IsEmailInUseByOtherUserAsync(string email, Guid currentUserId, CancellationToken cancellationToken)
        {
            try
            {
                return await _context.AspNetUser_DS
                    .AsNoTracking()
                    .Where(u => u.Email == email && u.UserID != currentUserId && !u.IsDeleted)
                    .AnyAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if email {Email} is in use by other user", email);
                return true; // Safe default - assume email is in use to prevent duplicates
            }
        }

        public async Task<AspNetUser?> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
        {
            try
            {
                return await _context.AspNetUser_DS
                    .AsNoTracking()
                    .Where(u => u.Email == email && !u.IsDeleted)
                    .FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by email {Email}", email);
                return null;
            }
        }

        public async Task UpdateUserPinAsync(Guid userID, string newPinHash, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _context.AspNetUser_DS
                    .FirstOrDefaultAsync(u => u.UserID == userID, cancellationToken);

                if (user == null)
                    throw new InvalidOperationException("Utente non trovato");

                user.QuickLoginPinHash = newPinHash;
                _context.AspNetUser_DS.Update(user);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating PIN for user {UserID}", userID);
                throw new InvalidOperationException("Errore durante l'aggiornamento del PIN");
            }
        }
    }

}
