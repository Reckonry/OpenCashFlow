using OpenCashFlow.Application.Setup.Ports;

namespace OpenCashFlow.Application.Setup.GetSetupStatus;

public sealed class GetSetupStatusUseCase(ISetupReader setupReader) : IGetSetupStatusUseCase
{
    public Task<SetupStatusResult> ExecuteAsync(CancellationToken cancellationToken = default)
        => setupReader.GetStatusAsync(cancellationToken);
}
