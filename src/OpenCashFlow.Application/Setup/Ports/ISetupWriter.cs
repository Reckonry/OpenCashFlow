using OpenCashFlow.Application.Setup.CompleteSetup;
using OpenCashFlow.Application.Setup.GetSetupStatus;

namespace OpenCashFlow.Application.Setup.Ports;

public interface ISetupWriter
{
    Task<SetupStatusResult> CompleteAsync(CompleteSetupCommand command, CancellationToken cancellationToken = default);
}
