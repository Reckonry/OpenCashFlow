namespace OpenCashFlow.Application.Auth.ResetPassword;

public interface IResetPasswordUseCase
{
    Task<ResetPasswordResult> ExecuteAsync(ResetPasswordCommand command, CancellationToken cancellationToken = default);
}
