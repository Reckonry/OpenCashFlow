using OpenCashFlow.Application.Cash.Integrity.Models;

namespace OpenCashFlow.Application.Cash.Integrity.Ports;

public interface ICashAccountWriter
{
    Task<CashAccountResult?> CreateAsync(CashAccountResult account, CancellationToken cancellationToken = default);

    Task<CashAccountResult?> UpdateAsync(CashAccountResult account, CancellationToken cancellationToken = default);

    Task<bool> DeactivateAsync(Guid tenantId, Guid cashAccountId, CancellationToken cancellationToken = default);
}
