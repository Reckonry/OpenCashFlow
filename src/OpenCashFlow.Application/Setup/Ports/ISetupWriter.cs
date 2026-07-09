using OpenCashFlow.Application.Setup.CompleteSetup;
using OpenCashFlow.Application.Setup.GetSetupStatus;

namespace OpenCashFlow.Application.Setup.Ports;

public interface ISetupWriter
{
    Task<SetupStatusResult> CompleteAsync(
        CompleteSetupCommand command,
        string temporaryAdminPassword,
        CancellationToken cancellationToken = default);
}
