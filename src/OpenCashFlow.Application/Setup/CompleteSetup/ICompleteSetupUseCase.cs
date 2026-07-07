namespace OpenCashFlow.Application.Setup.CompleteSetup;

public interface ICompleteSetupUseCase
{
    Task<CompleteSetupResult> ExecuteAsync(CompleteSetupCommand command, CancellationToken cancellationToken = default);
}
