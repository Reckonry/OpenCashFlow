namespace OpenCashFlow.Application.Auth.AccountConfirmation;

public interface IResendConfirmationUseCase
{
    Task<bool> ExecuteAsync(ResendConfirmationCommand command, CancellationToken cancellationToken = default);
}
