using OpenCashFlow.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using global::Shared.Core;
using global::Shared.DTOs;
using global::Shared.Models;
using global::Shared.Models.Core;
using global::Shared.Models.Identity;
using System.Text;

namespace OpenCashFlow.API.Services
{
    public partial class AuthenticationService : IAuthenticationService
    {
        public async Task<Core_RegistrationResult> RegistrationAsync(Register_DTO registration, CancellationToken cancellationToken)
        {
            // Required fields
            if (string.IsNullOrWhiteSpace(registration.Email) ||
                string.IsNullOrWhiteSpace(registration.Password) ||
                string.IsNullOrWhiteSpace(registration.CompanyName))
            {
                return Core_RegistrationResult.Failure(RegistrationError.MissingRequiredFields);
            }

            // Confirm password must match
            if (!string.Equals(registration.Password, registration.ConfirmPassword))
            {
                return Core_RegistrationResult.Failure(RegistrationError.PasswordsDoNotMatch);
            }

            // Privacy policy must be accepted
            if (!registration.AcceptPrivacyPolicy)
            {
                return Core_RegistrationResult.Failure(RegistrationError.PrivacyPolicyNotAccepted);
            }

            // Email format validation (no spaces + valid format)
            if (!IsValidEmail(registration.Email))
            {
                return Core_RegistrationResult.Failure(RegistrationError.InvalidEmail);
            }

            // Password policy: min length, upper, lower, digit, special
            if (!IsStrongPassword(registration.Password))
            {
                return Core_RegistrationResult.Failure(RegistrationError.WeakPassword);
            }

            // Prevent duplicate users by email/username
            if (await _authenticationRepository.UserExistsAsync(registration.Email, cancellationToken))
            {
                return Core_RegistrationResult.Failure(RegistrationError.UsernameTaken);
            }



            // Unique identifiers
            Guid TenantID = Guid.NewGuid();
            Guid NewUserID = Guid.NewGuid();


            if(await _authenticationRepository.RegisterNewUserCompany(registration, TenantID, NewUserID, cancellationToken) == false)
                return Core_RegistrationResult.Failure(RegistrationError.UnknownError);            

            #region EmailNotification
            bool EmailSent = false;

            var templatePath = Path.Combine(_env.ContentRootPath, "EmailTemplates", "RegistrationConfirmation.html");
            var html = await File.ReadAllTextAsync(templatePath, cancellationToken);

            html = html.Replace("{{UserName}}", registration.CompanyName ?? registration.FirstName)
                       .Replace("{{VerificationLinkTenantID}}", TenantID.ToString())
                       .Replace("{{VerificationLinkUserID}}", NewUserID.ToString());
            var RegistrationEmail = new EmailMessage("Conferma registrazione - OpenCashFlow", html.ToString())
            {
                FromName = "OpenCashFlow — Registrazione",
            };

            try
            {
                await _emailSender.SendEmailAsync(RegistrationEmail, registration.FirstName ?? registration.Email, registration.Email);
                EmailSent = true;
            }
            catch (Exception)
            {
                EmailSent = false;
            }
            #endregion

            #region SlackNotification
            var slackMessage = new StringBuilder()
                .AppendLine("🚀 *Nuovo cliente registrato* 🚀")
                .AppendLine($"> *Azienda:* {registration.CompanyName}")
                .AppendLine($"> *Email:* {registration.Email}");
            if (!EmailSent)
                slackMessage.AppendLine($"> :x: Non sono riuscito ad inviare la mail di registrazione");
            //await _slack.NotifyAsync(slackMessage.ToString());
            #endregion

            return Core_RegistrationResult.Ok();
        }

        public async Task<bool> ConfirmAccountAsync(Guid TenantID, Guid UserID, CancellationToken cancellationToken)
        {
            var confirmed = await _authenticationRepository.ConfirmAccountAsync(TenantID, UserID, cancellationToken);
            if (!confirmed)
                return false;

            // After confirmation: generate a unique PIN and email it to the user
            try
            {
                // 1) Generate a unique PIN per company
                var pin = await GenerateUniqueFastLoginPinAsync(TenantID, cancellationToken);

                // 2) Retrieve the user and update the fast PIN hash
                var user = await _context.AspNetUser_DS.FirstOrDefaultAsync(u => u.UserID == UserID, cancellationToken);
                if (user != null && !string.IsNullOrWhiteSpace(user.PasswordSalt) && string.IsNullOrWhiteSpace(user.QuickLoginPinHash))
                {
                    var hashedPin = global::Shared.Core.PasswordHasher.HashPasswordArgon2(pin, user.PasswordSalt);
                    user.QuickLoginPinHash = hashedPin;
                    _context.AspNetUser_DS.Update(user);
                    await _context.SaveChangesAsync(cancellationToken);

                    // 3) Send the PIN email using the existing template
                    try
                    {
                        var templatePath = Path.Combine(_env.ContentRootPath, "EmailTemplates", "EmployeeCreatedFastLoginPin.html");
                        string subject = "Your fast login PIN";
                        string displayName = string.Join(" ", new[] { user.UserFirstName, user.UserLastName }.Where(s => !string.IsNullOrWhiteSpace(s)));
                        if (string.IsNullOrWhiteSpace(displayName))
                            displayName = user.Email ?? user.UserName ?? "User";

                        string htmlContent;
                        if (File.Exists(templatePath))
                        {
                            htmlContent = await File.ReadAllTextAsync(templatePath, cancellationToken);
                            htmlContent = htmlContent
                                .Replace("{{FirstName}}", System.Net.WebUtility.HtmlEncode(user.UserFirstName ?? displayName))
                                .Replace("{{Email}}", System.Net.WebUtility.HtmlEncode(user.Email ?? string.Empty))
                                .Replace("{{FastLoginPin}}", System.Net.WebUtility.HtmlEncode(pin));
                        }
                        else
                        {
                            // Simple fallback if the template is not available
                            htmlContent = $@"<p>Hi {System.Net.WebUtility.HtmlEncode(displayName)},</p>
                                             <p>Your fast login PIN is: <strong>{System.Net.WebUtility.HtmlEncode(pin)}</strong>.</p>
                                             <p>Keep it safe and do not share it with anyone.</p>
                                             <p>– OpenCashFlow</p>";
                        }

                        var email = new EmailMessage(subject, htmlContent)
                        {
                            FromName = "OpenCashFlow — PIN"
                        };
                        var destination = user.Email ?? user.UserName ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(destination))
                            await _emailSender.SendEmailAsync(email, displayName, destination);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Impossibile inviare email PIN per l'utente {UserID}", UserID);
                    }
                }
            }
            catch (Exception ex)
            {
                // Do not block account confirmation if PIN generation/sending fails
                _logger.LogWarning(ex, "Errore durante generazione/invio PIN dopo conferma per utente {UserID}", UserID);
            }

            return true;
        }
    }
}

namespace OpenCashFlow.API.Services
{
    public partial class AuthenticationService
    {
        private async Task<string> GenerateUniqueFastLoginPinAsync(Guid companyId, CancellationToken cancellationToken)
        {
            const int maxAttempts = 50;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // 5-digit numeric PIN (00000-99999)
                var pin = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, 100000).ToString("D5");
                var inUse = await _employeeRepository.IsFastLoginPinInUseAsync(companyId, pin, cancellationToken);
                if (!inUse)
                    return pin;
            }
            throw new InvalidOperationException("Unable to generate a unique PIN. Please try again.");
        }
        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            if (email.Contains(' ')) return false;
            try
            {
                // System.Net.Mail is permissive enough for typical formats
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
    }
}
