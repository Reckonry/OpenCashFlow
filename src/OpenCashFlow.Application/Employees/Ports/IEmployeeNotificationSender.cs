using OpenCashFlow.Application.Employees.Models;

namespace OpenCashFlow.Application.Employees.Ports;

public sealed record EmployeePinNotification(
    EmployeeCredentialSnapshot Employee,
    string TargetEmail,
    string Pin);

public sealed record EmployeeEmailChangedNotification(
    EmployeeCredentialSnapshot Employee,
    string OldEmail,
    string NewEmail);

public interface IEmployeeNotificationSender
{
    Task SendEmployeeCreatedPinAsync(EmployeePinNotification notification, CancellationToken cancellationToken = default);
    Task SendEmployeePinChangedAsync(EmployeePinNotification notification, CancellationToken cancellationToken = default);
    Task SendEmployeeEmailChangedAsync(EmployeeEmailChangedNotification notification, CancellationToken cancellationToken = default);
}
