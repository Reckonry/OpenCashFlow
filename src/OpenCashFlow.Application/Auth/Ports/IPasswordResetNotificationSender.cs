using OpenCashFlow.Application.Auth.Models;

namespace OpenCashFlow.Application.Auth.Ports;

public sealed record PasswordResetNotification(AuthUserCredential User, string ResetLink);

public interface IPasswordResetNotificationSender
{
    Task SendPasswordResetAsync(PasswordResetNotification notification, CancellationToken cancellationToken = default);
}
