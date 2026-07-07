using OpenCashFlow.Application.Auth.Ports;
using OpenCashFlow.Application.Employees.Ports;

namespace OpenCashFlow.Application.Auth.ResetPassword;

public sealed class ResetPasswordUseCase(
    IUserCredentialReader userReader,
    IUserPasswordWriter passwordWriter,
    IPasswordResetTokenStore tokenStore,
    IPasswordResetTokenGenerator tokenGenerator,
    IEmployeeCredentialService credentialService) : IResetPasswordUseCase
{
    public async Task<ResetPasswordResult> ExecuteAsync(ResetPasswordCommand command, CancellationToken cancellationToken = default)
    {
        if (command.UserID == Guid.Empty || string.IsNullOrWhiteSpace(command.Token) || string.IsNullOrWhiteSpace(command.NewPassword))
        {
            return new ResetPasswordResult(false, "Invalid request");
        }

        var decodedToken = tokenGenerator.DecodeTokenOrPassthrough(command.Token);
        if (!await tokenStore.HasValidTokenAsync(command.UserID, decodedToken, cancellationToken))
        {
            return new ResetPasswordResult(false, "Invalid or expired token");
        }

        var user = await userReader.GetByIdAsync(command.UserID, cancellationToken);
        if (user is null)
        {
            return new ResetPasswordResult(false, "User not found");
        }

        await passwordWriter.UpdatePasswordHashAsync(command.UserID, credentialService.HashSecret(command.NewPassword, user.PasswordSalt), cancellationToken);
        await tokenStore.InvalidateAsync(command.UserID, decodedToken, cancellationToken);
        return new ResetPasswordResult(true, null);
    }
}
