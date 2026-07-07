using OpenCashFlow.Application.Abstractions;
using OpenCashFlow.Application.Auth.Ports;
using System.Net;

namespace OpenCashFlow.Infrastructure.Auth;

public sealed class PasswordResetNotificationSender(IEmailSender emailSender) : IPasswordResetNotificationSender
{
    public Task SendPasswordResetAsync(PasswordResetNotification notification, CancellationToken cancellationToken = default)
    {
        var userName = WebUtility.HtmlEncode(notification.User.UserFirstName);
        var resetLink = WebUtility.HtmlEncode(notification.ResetLink);
        var html = $@"<p>Hi {userName},</p>
<p>You requested a password reset for OpenCashFlow.</p>
<p><a href=""{resetLink}"">Reset your password</a></p>
<p>If you did not request this, you can ignore this email.</p>
<p>- OpenCashFlow</p>";

        return emailSender.SendEmailAsync(
            new EmailMessage("Reset Password - OpenCashFlow", html) { FromName = "OpenCashFlow - Password Reset" },
            notification.User.UserFirstName,
            notification.User.Email);
    }
}
