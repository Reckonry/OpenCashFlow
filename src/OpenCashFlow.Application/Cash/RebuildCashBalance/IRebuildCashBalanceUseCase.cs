namespace OpenCashFlow.Application.Cash.RebuildCashBalance;

public interface IRebuildCashBalanceUseCase
{
    Task ExecuteAsync(Guid companyId, CancellationToken cancellationToken = default);
}
