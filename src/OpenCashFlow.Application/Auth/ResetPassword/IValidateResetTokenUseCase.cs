namespace OpenCashFlow.Application.Auth.ResetPassword;

public interface IValidateResetTokenUseCase
{
    Task<ValidateResetTokenResult> ExecuteAsync(ValidateResetTokenCommand command, CancellationToken cancellationToken = default);
}
