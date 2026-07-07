using OpenCashFlow.Application.Employees.Ports;

namespace OpenCashFlow.Application.Employees.ResendPin;

public sealed class ResendEmployeePinUseCase(
    IEmployeeReader employeeReader,
    IEmployeeWriter employeeWriter,
    IEmployeeCredentialService credentialService,
    IEmployeePinService pinService,
    IEmployeeNotificationSender notificationSender) : IResendEmployeePinUseCase
{
    public async Task<bool> ExecuteAsync(ResendEmployeePinCommand command, CancellationToken cancellationToken = default)
    {
        if (command.TenantID == Guid.Empty || command.UserID == Guid.Empty)
        {
            return false;
        }

        var employee = await employeeReader.GetEmployeeCredentialAsync(command.UserID, command.TenantID, cancellationToken);
        if (employee is null || string.IsNullOrWhiteSpace(employee.Email) || string.IsNullOrWhiteSpace(employee.PasswordSalt))
        {
            return false;
        }

        try
        {
            var pin = await pinService.GenerateUniquePinAsync(command.TenantID, cancellationToken);
            await employeeWriter.UpdatePinHashAsync(command.UserID, credentialService.HashSecret(pin, employee.PasswordSalt), cancellationToken);
            await notificationSender.SendEmployeePinChangedAsync(new EmployeePinNotification(employee, employee.Email, pin), cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
