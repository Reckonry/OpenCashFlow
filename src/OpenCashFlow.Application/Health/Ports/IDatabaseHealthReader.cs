namespace OpenCashFlow.Application.Health.Ports;

public interface IDatabaseHealthReader
{
    Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);
}

