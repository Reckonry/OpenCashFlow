using AutoMapper;
using OpenCashFlow.API.Repositories.Interfaces;
using OpenCashFlow.API.Services.Interfaces;
using global::Shared.Core;
using global::Shared.DTOs.Employees;
using global::Shared.Models;
using global::Shared.Models.Identity;
using global::Shared.Services.Interfaces;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Text;

namespace OpenCashFlow.API.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IAuthenticationService _authenticationService;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _env;

        // Username policy / limits
        private const int PolicyMaxUserNameLength = 40;     // soft UX limit
        private const int DbMaxUserNameLength = 256;        // hard DB column size (varchar(256))
        private static readonly int MaxUserNameLength = Math.Min(PolicyMaxUserNameLength, DbMaxUserNameLength);
        private const string DefaultUserBase = "user";

        // Sanitization helpers
        private static readonly Regex AllowedUsernameCharsRegex = new("[^a-z0-9._-]+", RegexOptions.Compiled);
        private static readonly Regex RepeatSeparatorsRegex = new("[._-]{2,}", RegexOptions.Compiled);
        private static readonly Regex EdgeSeparatorsRegex = new("^[._-]+|[._-]+$", RegexOptions.Compiled);

        public EmployeeService(IEmployeeRepository EmployeeRepository, IAuthenticationService authenticationService, IMapper mapper, IEmailSender emailSender, IWebHostEnvironment env)
        {
            _employeeRepository = EmployeeRepository;
            _authenticationService = authenticationService;
            _mapper = mapper;
            _emailSender = emailSender;
            _env = env;
        }

        public async Task<IEnumerable<Employee_List_DTO>?> GetEmployeesAsync(CancellationToken cancellationToken)
        {
            return _mapper.Map<IEnumerable<Employee_List_DTO>>(
                await _employeeRepository.GetEmployeesAsync(_authenticationService.GetTenantID(), cancellationToken));
        }

        public async Task<Employee_Detail_DTO?> GetEmployeesByIDAsync(Guid UserID, CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(UserID, _authenticationService.GetTenantID(), cancellationToken);
            return _mapper.Map<Employee_Detail_DTO?>(employee);
        }

        public async Task<Employee_Detail_DTO> CreateEmployeeAsync(Employee_Create_DTO model, CancellationToken cancellationToken)
        {
            // Server-side validation for required fields not enforced by attributes (e.g., Guid default)
            if (string.IsNullOrWhiteSpace(model.Email))
                throw new ArgumentException("Email is required", nameof(model.Email));
            if (string.IsNullOrWhiteSpace(model.TmpNewPassword))
                throw new ArgumentException("Password is required", nameof(model.TmpNewPassword));

            // Password policy validation: min 8 chars, upper, lower, digit, special
            if (!IsStrongPassword(model.TmpNewPassword))
                throw new ArgumentException("La password deve contenere almeno 8 caratteri, una maiuscola, una minuscola, un numero e un carattere speciale", nameof(model.TmpNewPassword));

            // Il PIN non arriva più dal client: viene generato qui e verificata l'unicità per azienda
            var companyId = _authenticationService.GetTenantID();
            var generatedPin = await GenerateUniquePinAsync(companyId, cancellationToken);
            model.TmpFastLoginPin = generatedPin;

            // Pre-generate the UserID so we can derive a deterministic username with a short GUID tag
            model.UserID = Guid.NewGuid();

            // Username: human-friendly slug + short GUID tag (no DB lookup), obeys 40/256 limits
            model.UserName = GenerateUsernameFromGuid(model, model.UserID);

            // Verifica che il fast login PIN non sia già usato all'interno dell'azienda

            // Basic duplicate checks (by username/email)
            var existingByEmail = await _employeeRepository.GetUserByEmailAsync(model.Email, cancellationToken);
            if (existingByEmail != null)
                throw new InvalidOperationException("Email already exists");

            model.TenantID = companyId;
            model.CreatedBy = _authenticationService.GetUserID();
            model.IsApproved = true; // utente creato dal sistema -> già approvato
            await _employeeRepository.CreateEmployeeAsync(model, cancellationToken);
            var newEmployee = await GetEmployeesByIDAsync(model.UserID, cancellationToken);

            // Invia email con PIN al nuovo utente usando template HTML
            try
            {
                if (!string.IsNullOrWhiteSpace(model.Email))
                {
                    var displayName = string.Join(" ", new[] { model.UserFirstName, model.UserLastName }.Where(s => !string.IsNullOrWhiteSpace(s)));
                    var subject = "Benvenuto in OpenCashFlow - Il tuo PIN di accesso rapido";

                    var templatePath = Path.Combine(_env.ContentRootPath, "EmailTemplates", "EmployeeCreatedFastLoginPin.html");
                    string htmlContent;

                    if (File.Exists(templatePath))
                    {
                        htmlContent = await File.ReadAllTextAsync(templatePath);
                        htmlContent = htmlContent
                            .Replace("{{FirstName}}", System.Net.WebUtility.HtmlEncode(model.UserFirstName ?? displayName))
                            .Replace("{{Email}}", System.Net.WebUtility.HtmlEncode(model.Email))
                            .Replace("{{FastLoginPin}}", System.Net.WebUtility.HtmlEncode(generatedPin));
                    }
                    else
                    {
                        // Fallback se il template non fosse disponibile
                        htmlContent = $@"<p>Ciao {System.Net.WebUtility.HtmlEncode(displayName)},</p>
                                         <p>il tuo PIN di accesso rapido è: <strong>{generatedPin}</strong>.</p>
                                         <p>Conservalo con cura e non condividerlo con nessuno.</p>
                                         <p>– OpenCashFlow</p>";
                    }

                    await _emailSender.SendEmailAsync(
                        new EmailMessage(subject, htmlContent)
                        {
                            FromName = "OpenCashFlow — PIN"
                        },
                        displayName,
                        model.Email
                    );
                }
            }
            catch
            {
                // Non bloccare la creazione in caso di errore email
            }
            return newEmployee ?? throw new InvalidOperationException("Employee retrieval failed after creation.");
        }

        // Preferred: GUID-based deterministic username (human slug + short guid tag)
        private static string GenerateUsernameFromGuid(Employee_Create_DTO model, Guid userId)
        {
            var slug = BuildUsernameSeed(model);
            slug = SanitizeUsername(slug);
            if (string.IsNullOrWhiteSpace(slug)) slug = DefaultUserBase;

            var tag = userId.ToString("N").Substring(0, 6).ToLowerInvariant(); // 6-char tag
            var reserved = 1 + tag.Length; // '-' + tag
            var headLen = Math.Max(1, MaxUserNameLength - reserved);
            var head = Truncate(slug, headLen);
            return $"{head}-{tag}";
        }

        private static string BuildUsernameSeed(Employee_Create_DTO model)
        {
            var pieces = new[]
            {
                model.UserFirstName,
                model.UserLastName
            }
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s!.Trim());

            var fromNames = string.Join(".", pieces);
            if (!string.IsNullOrWhiteSpace(fromNames))
                return fromNames;

            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                var localPart = model.Email.Split('@')[0];
                if (!string.IsNullOrWhiteSpace(localPart))
                    return localPart;
            }

            return "user";
        }

        private static string SanitizeUsername(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Lowercase + remove diacritics
            var s = RemoveDiacritics(input).ToLowerInvariant();

            // Keep only allowed chars
            s = AllowedUsernameCharsRegex.Replace(s, string.Empty);

            // Collapse repeated separators (.. -> . , __ -> _ , -- -> -)
            s = RepeatSeparatorsRegex.Replace(s, m => m.Value[0].ToString());

            // Trim separators at the edges
            s = EdgeSeparatorsRegex.Replace(s, string.Empty);

            return s;
        }

        private static string RemoveDiacritics(string text)
        {
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(capacity: normalized.Length);
            foreach (var c in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
                return value;
            return value.Substring(0, maxLength);
        }

        public async Task<Employee_Update_Response_DTO> UpdateEmployeeAsync(Guid userID, Employee_Update_DTO model, CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(userID, _authenticationService.GetTenantID(), cancellationToken);
            if (employee == null)
                return new Employee_Update_Response_DTO
                {
                    Success = false,
                    Message = "Dipendente non trovato"
                };

            bool emailChanged = false;
            string? oldEmail = employee.User?.Email;

            // Validate email uniqueness if email is being changed
            if (!string.IsNullOrWhiteSpace(model.Email) &&
                !string.Equals(employee.User?.Email, model.Email, StringComparison.OrdinalIgnoreCase))
            {
                emailChanged = true;

                // Validate email format
                if (!IsValidEmail(model.Email))
                    throw new ArgumentException("Formato email non valido", nameof(model.Email));

                // Check if email is already used by another user
                var emailInUse = await _employeeRepository.IsEmailInUseByOtherUserAsync(model.Email, userID, cancellationToken);
                if (emailInUse)
                    throw new InvalidOperationException("Email già utilizzata da un altro dipendente");
            }

            model.UserID = userID;
            model.TenantID = _authenticationService.GetTenantID();
            model.EditedBy = _authenticationService.GetUserID();
            model.DateEdit = DateTime.UtcNow;

            // If email changed, keep user active (no need for confirmation)
            if (emailChanged)
            {
                // Email is immediately active - no confirmation required
                model.EmailConfirmed = true;
                model.IsApproved = true;
            }

            await _employeeRepository.UpdateEmployeeAsync(model, cancellationToken);

            bool pinSentSuccessfully = true;
            // If email changed, resend PIN to new email
            if (emailChanged && !string.IsNullOrWhiteSpace(model.Email))
            {
                try
                {
                    await ResendPinToUpdatedEmailAsync(userID, model.Email, oldEmail, cancellationToken);
                }
                catch (Exception ex)
                {
                    pinSentSuccessfully = false;
                    // Log error but don't fail the update operation
                    // The employee data was successfully updated, email sending is secondary
                    Console.WriteLine($"[EmployeeService ERROR] Failed to send PIN to new email {model.Email}: {ex.Message}");
                }
            }

            // Build response message based on what happened
            string message;
            if (emailChanged)
            {
                if (pinSentSuccessfully)
                {
                    message = $"Dipendente aggiornato con successo. PIN inviato al nuovo indirizzo email: {model.Email}";
                }
                else
                {
                    message = $"Dipendente aggiornato con successo, ma si è verificato un errore nell'invio del PIN al nuovo indirizzo email: {model.Email}";
                }
            }
            else
            {
                message = "Dipendente aggiornato con successo";
            }

            return new Employee_Update_Response_DTO
            {
                Success = true,
                EmailChanged = emailChanged,
                PinSentSuccessfully = emailChanged ? pinSentSuccessfully : true, // If email didn't change, no PIN to send
                Message = message,
                NewEmail = emailChanged ? model.Email : null
            };
        }

        public async Task<bool> DeleteEmployeeAsync(Guid userID, CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(userID, _authenticationService.GetTenantID(), cancellationToken);
            if (employee?.User == null)
                return false;

            // Prevent deletion of the current user (self-deletion)
            var currentUserId = _authenticationService.GetUserID();
            if (userID == currentUserId)
                throw new InvalidOperationException("Non puoi eliminare il tuo stesso account");

            // Perform soft delete
            await _employeeRepository.DeleteEmployeeAsync(employee.User, cancellationToken);
            return true;
        }

        public async Task<bool> UpdateMyProfileAsync(Employee_MyProfile_Update_DTO model, CancellationToken cancellationToken)
        {
            var userId = _authenticationService.GetUserID();
            await _employeeRepository.UpdateMyProfileAsync(userId, model, cancellationToken);
            return true;
        }

        private async Task<string> GenerateUniquePinAsync(Guid companyId, CancellationToken cancellationToken)
        {
            const int maxAttempts = 50;
            for (int i = 0; i < maxAttempts; i++)
            {
                var pin = RandomNumberGenerator.GetInt32(0, 100000).ToString("D5");
                var inUse = await _employeeRepository.IsFastLoginPinInUseAsync(companyId, pin, cancellationToken);
                if (!inUse)
                    return pin;
            }
            throw new InvalidOperationException("Unable to generate a unique PIN. Please try again.");
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsStrongPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8) return false;
            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));
            return hasUpper && hasLower && hasDigit && hasSpecial;
        }

        public async Task<bool> ResendPinAsync(Guid userID, CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(userID, _authenticationService.GetTenantID(), cancellationToken);
            if (employee?.User == null || string.IsNullOrWhiteSpace(employee.User.Email))
                return false;

            try
            {
                await SendPinEmailAsync(employee.User, cancellationToken);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task ResendPinToUpdatedEmailAsync(Guid userID, string newEmail, string? oldEmail, CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(userID, _authenticationService.GetTenantID(), cancellationToken);
            if (employee?.User == null)
                throw new InvalidOperationException("Dipendente non trovato");

            // If PasswordSalt is missing, we can't proceed
            if (string.IsNullOrWhiteSpace(employee.User.PasswordSalt))
                throw new InvalidOperationException("PasswordSalt mancante - impossibile generare nuovo PIN");

            // Send PIN directly to new email (no confirmation required)
            await SendPinEmailToSpecificEmailAsync(employee.User, newEmail, cancellationToken);

            // Send notification to old email about the change
            if (!string.IsNullOrWhiteSpace(oldEmail) && !string.Equals(oldEmail, newEmail, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    await SendEmailChangeNotificationAsync(oldEmail, newEmail, employee.User, cancellationToken);
                }
                catch
                {
                    // Ignore notification failures to old email
                }
            }
        }


        private async Task SendPinEmailAsync(AspNetUser user, CancellationToken cancellationToken)
        {
            await SendPinEmailToSpecificEmailAsync(user, user.Email, cancellationToken);
        }

        private async Task SendPinEmailToSpecificEmailAsync(AspNetUser user, string targetEmail, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(targetEmail) || string.IsNullOrWhiteSpace(user.PasswordSalt))
                throw new InvalidOperationException("Dati utente incompleti per l'invio del PIN");

            // For security, we need to get the actual PIN from the hash
            // Since we can't decrypt, we'll need to generate a new PIN if email changed
            var companyId = _authenticationService.GetTenantID();
            var newPin = await GenerateUniquePinAsync(companyId, cancellationToken);

            // Update the user's PIN hash
            var newPinHash = PasswordHasher.HashPasswordArgon2(newPin, user.PasswordSalt);
            await _employeeRepository.UpdateUserPinAsync(user.UserID, newPinHash, cancellationToken);

            var displayName = string.Join(" ", new[] { user.UserFirstName, user.UserLastName }.Where(s => !string.IsNullOrWhiteSpace(s)));
            var subject = "OpenCashFlow - Nuovo PIN di accesso rapido";

            var templatePath = Path.Combine(_env.ContentRootPath, "EmailTemplates", "EmployeeCreatedFastLoginPin.html");
            string htmlContent;

            if (File.Exists(templatePath))
            {
                htmlContent = await File.ReadAllTextAsync(templatePath);
                htmlContent = htmlContent
                    .Replace("{{FirstName}}", System.Net.WebUtility.HtmlEncode(user.UserFirstName ?? displayName))
                    .Replace("{{Email}}", System.Net.WebUtility.HtmlEncode(targetEmail))
                    .Replace("{{FastLoginPin}}", System.Net.WebUtility.HtmlEncode(newPin));
            }
            else
            {
                htmlContent = $@"<p>Ciao {System.Net.WebUtility.HtmlEncode(displayName)},</p>
                                 <p>La tua email è stata aggiornata. Il tuo nuovo PIN di accesso rapido è: <strong>{newPin}</strong>.</p>
                                 <p>Conservalo con cura e non condividerlo con nessuno.</p>
                                 <p>– OpenCashFlow</p>";
            }

            await _emailSender.SendEmailAsync(
                new EmailMessage(subject, htmlContent)
                {
                    FromName = "OpenCashFlow — PIN"
                },
                displayName,
                targetEmail
            );
        }

        private async Task SendEmailChangeNotificationAsync(string oldEmail, string newEmail, AspNetUser user, CancellationToken cancellationToken)
        {
            var displayName = string.Join(" ", new[] { user.UserFirstName, user.UserLastName }.Where(s => !string.IsNullOrWhiteSpace(s)));
            var subject = "OpenCashFlow - Email modificata";

            var htmlContent = $@"<p>Ciao {System.Net.WebUtility.HtmlEncode(displayName)},</p>
                                 <p>Ti informiamo che la tua email in OpenCashFlow è stata modificata da <strong>{System.Net.WebUtility.HtmlEncode(oldEmail)}</strong> a <strong>{System.Net.WebUtility.HtmlEncode(newEmail)}</strong>.</p>
                                 <p>Un nuovo PIN di accesso è stato inviato al nuovo indirizzo email.</p>
                                 <p>Se non hai richiesto questa modifica, contatta immediatamente l'amministratore.</p>
                                 <p>– OpenCashFlow</p>";

            await _emailSender.SendEmailAsync(
                new EmailMessage(subject, htmlContent)
                {
                    FromName = "OpenCashFlow — Sicurezza"
                },
                displayName,
                oldEmail
            );
        }

    }
}
