using OpenCashFlow.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using global::Shared.Core;
using global::Shared.Data;
using global::Shared.DTOs;
using global::Shared.Models;
using global::Shared.Models.Core;
using global::Shared.Models.Identity;
using static global::Shared.Logging.LogEvents;

namespace OpenCashFlow.API.Repositories
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthenticationRepository> _logger;

        public AuthenticationRepository(ApplicationDbContext context, ILogger<AuthenticationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<AspNetUser?> GetUserByUsernameAndPasswordAsync(string username, string password, CancellationToken cancellationToken)
        {
            // 1) Recupera l’utente (solo lettura)
            var user = await _context.AspNetUser_DS.Include(u => u.Roles).ThenInclude(u => u.AspNetRole)
                .AsNoTracking().FirstOrDefaultAsync(u =>
                    u.UserName == username ||
                    u.Email == username ||
                    u.PhoneNumber == username,
                cancellationToken);

            if (user == null)
                return null; // Utente non trovato

            // 2) Verifica che salt e hash non siano null o vuoti
            if (string.IsNullOrWhiteSpace(user.PasswordSalt) ||
                string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                // (Opzionale) log per utenti malformati
                _logger?.LogWarning(
                    "Login fallito per {Username}: salt/hash mancanti", username);

                return null;
            }

            // 3) Calcola l’hash del password inserito
            var hashedPassword = PasswordHasher.HashPasswordArgon2(
                password,
                user.PasswordSalt);

            // 4) Confronta e restituisci
            return hashedPassword == user.PasswordHash ? user : null;
        }

        public async Task<AspNetUser?> GetUserByCompanyIDAndPinAsync(Guid CompanyID, string Pin, CancellationToken cancellationToken)
        {
            var staffList = await _context.Company_Staff_DS.AsNoTracking().Include(cs => cs.User)
                .Where(cs => cs.TenantID == CompanyID).ToListAsync(cancellationToken);

            foreach (var staff in staffList)
            {
                var user = staff.User;
                if (user == null) continue;
                if (string.IsNullOrEmpty(user.PasswordSalt)) continue;
                var hashedPin = PasswordHasher.HashPasswordArgon2(Pin, user.PasswordSalt);

                if (user.QuickLoginPinHash == hashedPin) return user;
            }

            // Se non ho trovato corrispondenza, restituisco null
            return null;
        }

        public async Task<bool> UserExistsAsync(string username, CancellationToken cancellationToken)
        {
            return await _context.AspNetUser_DS.AsNoTracking()
                .AnyAsync(u => u.UserName == username.Trim() || u.Email == username.Trim(), cancellationToken);
        }

        public async Task<AspNetUser?> GetUserByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(usernameOrEmail)) return null;
            var input = usernameOrEmail.Trim();
            return await _context.AspNetUser_DS.AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserName == input || u.Email == input, cancellationToken);
        }

        public async Task<bool> RegisterNewUserCompany(Register_DTO registration, Guid TenantID, Guid UserID, CancellationToken cancellationToken)
        {

            // Creazione utente di base
            var salt = PasswordHasher.GenerateSalt();
            AspNetUser aspNetUser = new()
            {
                UserID = UserID,
                UserName = registration.Email,
                Email = registration.Email,
                EmailConfirmed = false,
                UserFirstName = registration.FirstName ?? registration.CompanyName,
                UserLastName = registration.LastName,
                PasswordSalt = salt,
                PasswordHash = PasswordHasher.HashPasswordArgon2(registration.Password, salt),
                PrivacyPolicyAcepted = registration.AcceptPrivacyPolicy,
                PrivacyPolicyAcceptedDate = registration.AcceptPrivacyPolicy ? DateTime.UtcNow : null
            };

            AspNetUserRole aspNetUserRole = new() { UserID = UserID, RoleID = Configuration.AdministratorRoleID };

            Company company = new()
            {
                TenantID = TenantID,
                CompanyName = registration.CompanyName,
                MaxUsers = 1000,
                StartingContract = DateTime.UtcNow,
                EndingContract = DateTime.UtcNow.AddDays(30),
                IsActive = true,
                MasterPassword = Guid.NewGuid().ToString("N"),
                CompanySecret = Guid.NewGuid().ToString("N"),
                CreatedBy = UserID,
            };

            Company_Contact_Email company_Contact_Email = new()
            {
                ContactEmailID = Guid.NewGuid(),
                TenantID = TenantID,
                Email = registration.Email,
                CreatedBy = UserID,
            };

            Company_Staff company_Staff = new()
            {
                UserID = UserID,
                TenantID = TenantID,
                CreatedBy = UserID,
            };

            try
            {
                // Salvataggio su database
                await _context.AspNetUser_DS.AddAsync(aspNetUser, cancellationToken);
                await _context.AspNetUserRole_DS.AddAsync(aspNetUserRole, cancellationToken);
                await _context.Company_DS.AddAsync(company, cancellationToken);
                await _context.AddAsync(company_Contact_Email, cancellationToken);
                await _context.Company_Staff_DS.AddAsync(company_Staff, cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        public async Task<bool> ConfirmAccountAsync(Guid TenantID, Guid UserID, CancellationToken cancellationToken)
        {
            //todo: limitare a solo chi e IsApproved = false o EmailConfirmed= false ?
            var user = await _context.AspNetUser_DS.FirstOrDefaultAsync(u => u.UserID == UserID, cancellationToken);
            if (user == null) return false;
            var staff = await _context.Company_Staff_DS.FirstOrDefaultAsync(cs => cs.UserID == UserID && cs.TenantID == TenantID, cancellationToken);
            if (staff == null) return false;
            user.EmailConfirmed = true;
            user.IsApproved = true;
            _context.AspNetUser_DS.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<IEnumerable<AspNetUserClaim>> GetUserClaimsAsync(Guid UserID, CancellationToken cancellationToken)
        {
            return await _context.AspNetUserClaim_DS.AsNoTracking().Where(uc => uc.UserID == UserID).ToListAsync(cancellationToken);
        }
    }
}
