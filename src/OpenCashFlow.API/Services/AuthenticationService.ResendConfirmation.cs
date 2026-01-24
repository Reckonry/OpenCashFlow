using OpenCashFlow.API.Services.Interfaces;
using global::Shared.Models;

namespace OpenCashFlow.API.Services
{
    public partial class AuthenticationService : IAuthenticationService
    {
        public async Task<bool> ResendConfirmationAsync(string usernameOrEmail, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(usernameOrEmail)) return true; // avoid enumeration

                var user = await _authenticationRepository.GetUserByUsernameOrEmailAsync(usernameOrEmail, cancellationToken);
                if (user == null) return true; // do not reveal user existence

                // If already approved/confirmed, report success silently
                if (user.IsApproved && user.EmailConfirmed) return true;

                var TenantID = await _companyRepository.GetUserTenantIDAsync(user.UserID, cancellationToken);
                if (TenantID == null) return true; // avoid leaking info

                // Prepare email using the same template as registration
                var templatePath = Path.Combine(_env.ContentRootPath, "EmailTemplates", "RegistrationConfirmation.html");
                if (!File.Exists(templatePath))
                {
                    _logger.LogWarning("RegistrationConfirmation template not found at {Template}", templatePath);
                    return true;
                }

                var html = await File.ReadAllTextAsync(templatePath, cancellationToken);
                html = html.Replace("{{UserName}}", user.UserFirstName ?? user.Email ?? user.UserName)
                           .Replace("{{VerificationLinkTenantID}}", TenantID.ToString()!)
                           .Replace("{{VerificationLinkUserID}}", user.UserID.ToString());

                var email = new EmailMessage("Conferma registrazione - OpenCashFlow", html)
                {
                    FromName = "OpenCashFlow — Registrazione"
                };
                try
                {
                    await _emailSender.SendEmailAsync(email, user.UserFirstName ?? user.Email ?? user.UserName, user.Email ?? user.UserName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to resend confirmation email to {Email}", user.Email);
                    // still return true to avoid enumeration/signaling
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ResendConfirmationAsync");
                // Avoid leaking details; still respond true
                return true;
            }
        }
    }
}

