using OpenCashFlow.Application.Cash.Ports;

namespace OpenCashFlow.Application.Cash.RebuildCashBalance;

public sealed class RebuildCashBalanceUseCase(ICashWriter cashWriter) : IRebuildCashBalanceUseCase
{
    public Task ExecuteAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        if (companyId == Guid.Empty)
        {
            throw new ArgumentException("Company id is required", nameof(companyId));
        }

        return cashWriter.RebuildBalanceAsync(companyId, cancellationToken);
    }
}
