using OpenCashFlow.Application.Cash.Ports;

namespace OpenCashFlow.Application.Cash.GetCashBalance;

public sealed class GetCashBalanceUseCase(ICashReader cashReader) : IGetCashBalanceUseCase
{
    public Task<decimal> ExecuteAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        if (companyId == Guid.Empty)
        {
            throw new ArgumentException("Company id is required", nameof(companyId));
        }

        return cashReader.GetCurrentAsync(companyId, cancellationToken);
    }
}
