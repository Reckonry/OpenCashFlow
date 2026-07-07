using OpenCashFlow.Contracts.Cash;

namespace OpenCashFlow.API.Services.Interfaces
{
    public interface ICashService
    {
        Task ApplyPaymentAsync(Guid companyId, Guid paymentId, decimal amount, string userId, CancellationToken ct);
        Task ReapplyPaymentAsync(Guid companyId, Guid paymentId, decimal amount, string userId, CancellationToken ct);
        Task RefundAsync(Guid companyId, Guid paymentId, decimal amount, string userId, CancellationToken ct);
        Task VoidAsync(Guid companyId, Guid paymentId, decimal originalAmount, string userId, CancellationToken ct);
        Task UpdatePaymentAsync(Guid companyId, Guid paymentId, decimal originalDelta, decimal newDelta, string userId, CancellationToken ct);

        Task AdminAdjustAsync(Guid companyId, decimal delta, string reason, string userId, CancellationToken ct);
        Task RebuildBalanceAsync(Guid companyId, CancellationToken ct);

        Task<decimal> GetCurrentAsync(Guid companyId, CancellationToken ct);
        Task<IReadOnlyList<CashLedger>> GetLedgerAsync(Guid companyId, DateTimeOffset? from, DateTimeOffset? to, int skip, int take, CancellationToken ct);
    }
}
