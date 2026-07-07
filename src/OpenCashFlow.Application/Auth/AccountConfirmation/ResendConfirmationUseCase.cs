using OpenCashFlow.Application.Auth.Models;
using OpenCashFlow.Application.Auth.Ports;

namespace OpenCashFlow.Application.Auth.AccountConfirmation;

public sealed class ResendConfirmationUseCase(
    IAuthUserReader userReader,
    IRegistrationNotificationSender notificationSender) : IResendConfirmationUseCase
{
    public async Task<bool> ExecuteAsync(ResendConfirmationCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.UsernameOrEmail))
        {
            return true;
        }

        var user = await userReader.GetByUsernameEmailOrPhoneAsync(command.UsernameOrEmail, cancellationToken);
        if (user is null || user.IsApproved)
        {
            return true;
        }

        var tenantId = await userReader.GetUserTenantIdAsync(user.UserID, cancellationToken);
        if (tenantId is null)
        {
            return true;
        }

        try
        {
            await notificationSender.SendRegistrationConfirmationAsync(new RegistrationConfirmationNotification(new RegistrationDraft
            {
                CompanyName = user.UserFirstName,
                Email = user.Email,
                Password = string.Empty,
                FirstName = user.UserFirstName,
                LastName = user.UserLastName,
                AcceptPrivacyPolicy = true
            }, tenantId.Value, user.UserID), cancellationToken);
        }
        catch
        {
            return true;
        }

        return true;
    }
}
