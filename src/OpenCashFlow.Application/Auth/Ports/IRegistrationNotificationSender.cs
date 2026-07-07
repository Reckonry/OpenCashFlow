using OpenCashFlow.Application.Auth.Models;
using OpenCashFlow.Application.Employees.Models;

namespace OpenCashFlow.Application.Auth.Ports;

public sealed record RegistrationConfirmationNotification(RegistrationDraft Registration, Guid TenantID, Guid UserID);
public sealed record RegistrationFastLoginPinNotification(EmployeeCredentialSnapshot User, string Pin);

public interface IRegistrationNotificationSender
{
    Task SendRegistrationConfirmationAsync(RegistrationConfirmationNotification notification, CancellationToken cancellationToken = default);
    Task SendFastLoginPinAsync(RegistrationFastLoginPinNotification notification, CancellationToken cancellationToken = default);
}
