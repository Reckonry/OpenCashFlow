using OpenCashFlow.Application.Cash.Integrity.Models;

namespace OpenCashFlow.Application.Cash.Integrity.Ports;

public interface ICashAccountReader
{
    Task<IReadOnlyList<CashAccountResult>> GetAccountsAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<CashAccountResult?> GetByIdAsync(Guid tenantId, Guid cashAccountId, CancellationToken cancellationToken = default);

    Task<CashAccountResult?> GetDefaultAsync(Guid tenantId, string currency, CancellationToken cancellationToken = default);

    Task<CashAccountBalanceResult?> GetExpectedBalanceAsync(Guid tenantId, Guid cashAccountId, CancellationToken cancellationToken = default);
}
