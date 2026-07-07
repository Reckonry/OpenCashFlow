namespace OpenCashFlow.Application.Setup.GetSetupStatus;

public interface IGetSetupStatusUseCase
{
    Task<SetupStatusResult> ExecuteAsync(CancellationToken cancellationToken = default);
}
