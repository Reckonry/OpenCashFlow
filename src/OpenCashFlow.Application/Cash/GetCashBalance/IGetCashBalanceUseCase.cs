namespace OpenCashFlow.Application.Cash.GetCashBalance;

public interface IGetCashBalanceUseCase
{
    Task<decimal> ExecuteAsync(Guid companyId, CancellationToken cancellationToken = default);
}
