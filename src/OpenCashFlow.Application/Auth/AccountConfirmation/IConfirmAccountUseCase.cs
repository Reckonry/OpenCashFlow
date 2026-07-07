using OpenCashFlow.Application.Auth.Models;

namespace OpenCashFlow.Application.Auth.AccountConfirmation;

public interface IConfirmAccountUseCase
{
    Task<AccountConfirmationResult> ExecuteAsync(ConfirmAccountCommand command, CancellationToken cancellationToken = default);
}
