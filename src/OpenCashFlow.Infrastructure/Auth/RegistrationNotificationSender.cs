using OpenCashFlow.Application.Abstractions;
using OpenCashFlow.Application.Auth.Ports;
using System.Net;

namespace OpenCashFlow.Infrastructure.Auth;

public sealed class RegistrationNotificationSender(IEmailSender emailSender) : IRegistrationNotificationSender
{
    public Task SendRegistrationConfirmationAsync(RegistrationConfirmationNotification notification, CancellationToken cancellationToken = default)
    {
        var html = $@"<p>Hi {Html(notification.Registration.FirstName ?? notification.Registration.CompanyName)},</p>
<p>Confirm your OpenCashFlow registration.</p>
<p>Tenant ID: <strong>{notification.TenantID}</strong></p>
<p>User ID: <strong>{notification.UserID}</strong></p>
<p>- OpenCashFlow</p>";

        return emailSender.SendEmailAsync(
            new EmailMessage("Conferma registrazione - OpenCashFlow", html) { FromName = "OpenCashFlow - Registrazione" },
            notification.Registration.FirstName ?? notification.Registration.Email,
            notification.Registration.Email);
    }

    public Task SendFastLoginPinAsync(RegistrationFastLoginPinNotification notification, CancellationToken cancellationToken = default)
    {
        var html = $@"<p>Hi {Html(notification.User.UserFirstName)},</p>
<p>Your fast login PIN is: <strong>{Html(notification.Pin)}</strong>.</p>
<p>Keep it safe and do not share it with anyone.</p>
<p>- OpenCashFlow</p>";

        return emailSender.SendEmailAsync(
            new EmailMessage("Your fast login PIN", html) { FromName = "OpenCashFlow - PIN" },
            DisplayName(notification.User.UserFirstName, notification.User.UserLastName),
            notification.User.Email);
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
