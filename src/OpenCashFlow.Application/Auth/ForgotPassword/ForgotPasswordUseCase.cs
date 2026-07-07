using OpenCashFlow.Application.Auth.Ports;

namespace OpenCashFlow.Application.Auth.ForgotPassword;

public sealed class ForgotPasswordUseCase(
    IUserCredentialReader userReader,
    IPasswordResetTokenStore tokenStore,
    IPasswordResetTokenGenerator tokenGenerator,
    IPasswordResetNotificationSender notificationSender) : IForgotPasswordUseCase
{
    public async Task<ForgotPasswordResult> ExecuteAsync(ForgotPasswordCommand command, CancellationToken cancellationToken = default)
    {
        var user = command.UserID.HasValue
            ? await userReader.GetByIdAsync(command.UserID.Value, cancellationToken)
            : string.IsNullOrWhiteSpace(command.EmailOrUserName)
                ? null
                : await userReader.GetByEmailOrUserNameAsync(command.EmailOrUserName, cancellationToken);

        if (user is null)
        {
            return new ForgotPasswordResult(false, null, null);
        }

        var token = tokenGenerator.GenerateToken();
        await tokenStore.CreateAsync(user.UserID, token, DateTime.UtcNow.AddMinutes(command.TokenTtlMinutes <= 0 ? 30 : command.TokenTtlMinutes), cancellationToken);

        var resetLink = $"{command.AppBaseUrl.TrimEnd('/')}/reset-password?token={tokenGenerator.EncodeToken(token)}";
        try
        {
            await notificationSender.SendPasswordResetAsync(new PasswordResetNotification(user, resetLink), cancellationToken);
        }
        catch
        {
            // Token creation must not depend on SMTP availability in a self-hosted installation.
        }

        return new ForgotPasswordResult(true, user.UserID, user.UserName);
    }
}
