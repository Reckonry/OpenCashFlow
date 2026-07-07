namespace OpenCashFlow.Application.Auth.ForgotPassword;

public interface IForgotPasswordUseCase
{
    Task<ForgotPasswordResult> ExecuteAsync(ForgotPasswordCommand command, CancellationToken cancellationToken = default);
}
