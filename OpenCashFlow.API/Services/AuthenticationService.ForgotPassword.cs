using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using OpenCashFlow.API.Services.Interfaces;

namespace OpenCashFlow.API.Services
{
    public partial class AuthenticationService : IAuthenticationService
    {
        /// <summary>
        /// Gestisce la richiesta di reset password tramite email
        /// </summary>
        /// <param name="email">Email dell'utente che richiede il reset</param>
        /// <param name="cancellationToken">Token di cancellazione</param>
        public async Task ForgotPasswordAsync(string email, CancellationToken cancellationToken)
        {
            // Cerca l'utente nel database
            var user = await _employeeRepository.GetUserAsync(email, cancellationToken);
            if (user == null) return; // Non espongo info per sicurezza - comportamento identico sia che l'utente esista o no

            // Genera un token di reset univoco
            var token = Guid.NewGuid().ToString("N");

            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            // Salva il token nel database con scadenza configurabile
            var ttlMinutes = int.TryParse(Environment.GetEnvironmentVariable("PASSWORD_RESET_TOKEN_MINUTES"), out var m) ? m : 30;
            await _employeeRepository.CreateResetTokenAsync(user.UserID, token, DateTime.UtcNow.AddMinutes(ttlMinutes), cancellationToken);

            // Crea il link di reset per il frontend
            var appBaseUrl = _configuration["AppUrl"] ?? "https://app.opencashflow.cloud";
            var resetLink = $"{appBaseUrl.TrimEnd('/')}/reset-password?token={encodedToken}";

            // Carica il template HTML professionale e sostituisce i placeholder
            var emailContent = await _emailTemplateService.GetForgotPasswordTemplateAsync(user.UserFirstName, resetLink);

            var emailMessage = new global::Shared.Models.EmailMessage("Reset Password - OpenCashFlow", emailContent)
            {
                FromName = "OpenCashFlow — Password Reset"
            };

            // Invia l'email utilizzando il servizio di invio configurato
            try
            {
                await _emailSender.SendEmailAsync(emailMessage, user.UserFirstName, user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
                throw new Exception("Errore nell'invio dell'email. Riprova più tardi.");
            }
        }
        /// <summary>
        /// Overload del metodo ForgotPassword che accetta UserID invece di email
        /// Utilizzato quando si conosce già l'ID dell'utente
        /// </summary>
        /// <param name="UserID">ID dell'utente che richiede il reset</param>
        /// <param name="cancellationToken">Token di cancellazione</param>
        public async Task ForgotPasswordAsync(Guid UserID, CancellationToken cancellationToken)
        {
            // Cerca l'utente nel database tramite ID
            var user = await _employeeRepository.GetUserByIdAsync(UserID, cancellationToken);
            if (user == null) return; // Non espongo info per sicurezza

            // Genera un token di reset univoco
            var token = Guid.NewGuid().ToString("N");

            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            // Salva il token nel database con scadenza configurabile
            var ttlMinutes = int.TryParse(Environment.GetEnvironmentVariable("PASSWORD_RESET_TOKEN_MINUTES"), out var m2) ? m2 : 30;
            await _employeeRepository.CreateResetTokenAsync(UserID, token, DateTime.UtcNow.AddMinutes(ttlMinutes), cancellationToken);

            // Crea il link di reset per il frontend
            var appBaseUrl = _configuration["AppUrl"] ?? "https://app.opencashflow.cloud";
            var resetLink = $"{appBaseUrl.TrimEnd('/')}/reset-password?token={encodedToken}";

            // Carica il template HTML professionale e sostituisce i placeholder
            var emailContent = await _emailTemplateService.GetForgotPasswordTemplateAsync(user.UserFirstName, resetLink);

            var emailMessage = new global::Shared.Models.EmailMessage("Reset Password - OpenCashFlow", emailContent)
            {
                FromName = "OpenCashFlow — Password Reset"
            };

            // Invia l'email utilizzando il servizio di invio configurato
            try
            {
                await _emailSender.SendEmailAsync(emailMessage, user.UserFirstName, user.Email);
                _logger.LogInformation("Password reset email sent to UserID {UserID}", UserID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to UserID {UserID}", UserID);
                throw new Exception("Errore nell'invio dell'email. Riprova più tardi.");
            }
        }

        /// <summary>
        /// Completa il processo di reset password utilizzando il token ricevuto via email
        /// </summary>
        /// <param name="UserID">ID dell'utente</param>
        /// <param name="token">Token di reset ricevuto via email</param>
        /// <param name="newPassword">Nuova password da impostare</param>
        /// <param name="cancellationToken">Token di cancellazione</param>
        public async Task ResetPasswordAsync(Guid UserID, string token, string newPassword, CancellationToken cancellationToken)
        {
            // Verifica che il token sia valido e non scaduto
            var resetToken = await _employeeRepository.HasValidTokenAsync(UserID, token, cancellationToken);
            if (resetToken == false) throw new Exception("Token non valido o scaduto");

            // Aggiorna la password dell'utente nel database
            await _employeeRepository.UpdatePasswordAsync(UserID, newPassword, cancellationToken);
        }
    }
}
