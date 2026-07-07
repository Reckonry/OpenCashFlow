using OpenCashFlow.Application.Auth.Ports;

namespace OpenCashFlow.Application.Auth.ResetPassword;

public sealed class ValidateResetTokenUseCase(
    IPasswordResetTokenStore tokenStore,
    IPasswordResetTokenGenerator tokenGenerator) : IValidateResetTokenUseCase
{
    public async Task<ValidateResetTokenResult> ExecuteAsync(ValidateResetTokenCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Token))
        {
            return new ValidateResetTokenResult(false, false, null);
        }

        var decodedToken = tokenGenerator.DecodeTokenOrPassthrough(command.Token);
        var validation = await tokenStore.ValidateAsync(decodedToken, cancellationToken);
        return new ValidateResetTokenResult(validation.IsValid, validation.IsExpired, validation.UserID);
    }
}
