using OpenCashFlow.Application.Abstractions;
using OpenCashFlow.Application.Employees.Ports;
using System.Net;

namespace OpenCashFlow.Infrastructure.Employees;

public sealed class EmployeeNotificationSender(IEmailSender emailSender) : IEmployeeNotificationSender
{
    public Task SendEmployeeCreatedPinAsync(EmployeePinNotification notification, CancellationToken cancellationToken = default)
    {
        var subject = "Welcome to OpenCashFlow - Your fast login PIN";
        var body = PinBody(notification.Employee.UserFirstName, notification.TargetEmail, notification.Pin);
        return emailSender.SendEmailAsync(new EmailMessage(subject, body) { FromName = "OpenCashFlow - PIN" }, DisplayName(notification), notification.TargetEmail);
    }

    public Task SendEmployeePinChangedAsync(EmployeePinNotification notification, CancellationToken cancellationToken = default)
    {
        var subject = "OpenCashFlow - New fast login PIN";
        var body = PinBody(notification.Employee.UserFirstName, notification.TargetEmail, notification.Pin);
        return emailSender.SendEmailAsync(new EmailMessage(subject, body) { FromName = "OpenCashFlow - PIN" }, DisplayName(notification), notification.TargetEmail);
    }

    public Task SendEmployeeEmailChangedAsync(EmployeeEmailChangedNotification notification, CancellationToken cancellationToken = default)
    {
        var displayName = DisplayName(notification.Employee.UserFirstName, notification.Employee.UserLastName);
        var body = $@"<p>Hi {Html(displayName)},</p>
<p>We are letting you know that your email in OpenCashFlow has been changed from <strong>{Html(notification.OldEmail)}</strong> to <strong>{Html(notification.NewEmail)}</strong>.</p>
<p>A new access PIN has been sent to the new email address.</p>
<p>If you did not request this change, contact the administrator immediately.</p>
<p>- OpenCashFlow</p>";

        return emailSender.SendEmailAsync(new EmailMessage("OpenCashFlow - Email changed", body) { FromName = "OpenCashFlow - Security" }, displayName, notification.OldEmail);
    }

    private static string PinBody(string firstName, string email, string pin)
    {
        return $@"<p>Hi {Html(firstName)},</p>
<p>Your fast login PIN is: <strong>{Html(pin)}</strong>.</p>
<p>Keep it safe and do not share it with anyone.</p>
<p>- OpenCashFlow</p>";
    }

    private static string DisplayName(EmployeePinNotification notification)
    {
        return DisplayName(notification.Employee.UserFirstName, notification.Employee.UserLastName);
    }

    private static string DisplayName(string firstName, string? lastName)
    {
        return string.Join(" ", new[] { firstName, lastName }.Where(s => !string.IsNullOrWhiteSpace(s)));
    }

    private static string Html(string? value)
    {
        return WebUtility.HtmlEncode(value ?? string.Empty);
    }
}
