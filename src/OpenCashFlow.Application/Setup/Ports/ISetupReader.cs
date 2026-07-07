using OpenCashFlow.Application.Setup.GetSetupStatus;

namespace OpenCashFlow.Application.Setup.Ports;

public interface ISetupReader
{
    Task<SetupStatusResult> GetStatusAsync(CancellationToken cancellationToken = default);
}
