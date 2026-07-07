using OpenCashFlow.Application.Auth.Models;
using OpenCashFlow.Application.Auth.Ports;

namespace OpenCashFlow.Application.Auth.Register;

public sealed class RegisterUseCase(
    IAuthUserReader userReader,
    IAuthUserWriter userWriter,
    IAuthPasswordVerifier passwordVerifier,
    IRegistrationNotificationSender notificationSender) : IRegisterUseCase
{
    public async Task<RegistrationResult> ExecuteAsync(RegisterCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Email) ||
            string.IsNullOrWhiteSpace(command.Password) ||
            string.IsNullOrWhiteSpace(command.CompanyName))
        {
            return new RegistrationResult(false, RegistrationFailure.MissingRequiredFields);
        }

        if (!string.Equals(command.Password, command.ConfirmPassword))
        {
            return new RegistrationResult(false, RegistrationFailure.PasswordsDoNotMatch);
        }

        if (!command.AcceptPrivacyPolicy)
        {
            return new RegistrationResult(false, RegistrationFailure.PrivacyPolicyNotAccepted);
        }

        if (!IsValidEmail(command.Email))
        {
            return new RegistrationResult(false, RegistrationFailure.InvalidEmail);
        }

        if (!passwordVerifier.IsStrongPassword(command.Password))
        {
            return new RegistrationResult(false, RegistrationFailure.WeakPassword);
        }

        if (await userReader.UserExistsAsync(command.Email, cancellationToken))
        {
            return new RegistrationResult(false, RegistrationFailure.UsernameTaken);
        }

        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var salt = passwordVerifier.GenerateSalt();
        var draft = new RegistrationDraft
        {
            CompanyName = command.CompanyName,
            Email = command.Email,
            Password = command.Password,
            FirstName = command.FirstName,
            LastName = command.LastName,
            AcceptPrivacyPolicy = command.AcceptPrivacyPolicy
        };

        if (!await userWriter.CreateRegisteredCompanyAndUserAsync(draft, tenantId, userId, salt, passwordVerifier.HashPassword(command.Password, salt), cancellationToken))
        {
            return new RegistrationResult(false, RegistrationFailure.UnknownError);
        }

        try
        {
            await notificationSender.SendRegistrationConfirmationAsync(new RegistrationConfirmationNotification(draft, tenantId, userId), cancellationToken);
        }
        catch
        {
            // Registration remains valid when optional email delivery is unavailable.
        }

        return new RegistrationResult(true, RegistrationFailure.None);
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || email.Contains(' ')) return false;
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
}
