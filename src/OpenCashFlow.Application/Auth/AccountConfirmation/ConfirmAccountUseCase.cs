using OpenCashFlow.Application.Auth.Models;
using OpenCashFlow.Application.Auth.Ports;
using OpenCashFlow.Application.Employees.Models;
using OpenCashFlow.Application.Employees.Ports;

namespace OpenCashFlow.Application.Auth.AccountConfirmation;

public sealed class ConfirmAccountUseCase(
    IAuthUserWriter authUserWriter,
    IEmployeeReader employeeReader,
    IEmployeeWriter employeeWriter,
    IEmployeePinService pinService,
    IAuthPasswordVerifier passwordVerifier,
    IRegistrationNotificationSender notificationSender) : IConfirmAccountUseCase
{
    public async Task<AccountConfirmationResult> ExecuteAsync(ConfirmAccountCommand command, CancellationToken cancellationToken = default)
    {
        if (!await authUserWriter.ConfirmAccountAsync(command.TenantID, command.UserID, cancellationToken))
        {
            return new AccountConfirmationResult(false);
        }

        try
        {
            var user = await employeeReader.GetEmployeeCredentialAsync(command.UserID, command.TenantID, cancellationToken);
            if (user is not null && !string.IsNullOrWhiteSpace(user.PasswordSalt))
            {
                var pin = await pinService.GenerateUniquePinAsync(command.TenantID, cancellationToken);
                await employeeWriter.UpdatePinHashAsync(command.UserID, passwordVerifier.HashPassword(pin, user.PasswordSalt), cancellationToken);
                await notificationSender.SendFastLoginPinAsync(new RegistrationFastLoginPinNotification(user, pin), cancellationToken);
            }
        }
        catch
        {
            // Confirmation must not depend on optional PIN email delivery.
        }

        return new AccountConfirmationResult(true);
    }
}
