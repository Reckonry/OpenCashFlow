using OpenCashFlow.API.Services.Interfaces;
using OpenCashFlow.Application.Cash.CreateCashAdjustment;
using OpenCashFlow.Application.Cash.GetCashBalance;
using OpenCashFlow.Application.Cash.GetCashLedger;
using OpenCashFlow.Application.Cash.Ports;
using OpenCashFlow.Application.Cash.RebuildCashBalance;
using OpenCashFlow.Contracts.Cash;

namespace OpenCashFlow.API.Services
{
    public class CashService(
        ICashWriter cashWriter,
        IGetCashBalanceUseCase getCashBalanceUseCase,
        IGetCashLedgerUseCase getCashLedgerUseCase,
        ICreateCashAdjustmentUseCase createCashAdjustmentUseCase,
        IRebuildCashBalanceUseCase rebuildCashBalanceUseCase) : ICashService
    {
        public Task ApplyPaymentAsync(Guid companyId, Guid paymentId, decimal amount, string userId, CancellationToken ct)
            => cashWriter.ApplyPaymentAsync(companyId, paymentId, amount, userId, ct);

        public Task ReapplyPaymentAsync(Guid companyId, Guid paymentId, decimal amount, string userId, CancellationToken ct)
            => cashWriter.ReapplyPaymentAsync(companyId, paymentId, amount, userId, ct);

        public Task RefundAsync(Guid companyId, Guid paymentId, decimal amount, string userId, CancellationToken ct)
            => cashWriter.RefundAsync(companyId, paymentId, amount, userId, ct);

        public Task UpdatePaymentAsync(Guid companyId, Guid paymentId, decimal originalDelta, decimal newDelta, string userId, CancellationToken ct)
            => cashWriter.UpdatePaymentAsync(companyId, paymentId, originalDelta, newDelta, userId, ct);

        public Task VoidAsync(Guid companyId, Guid paymentId, decimal originalAmount, string userId, CancellationToken ct)
            => cashWriter.VoidAsync(companyId, paymentId, originalAmount, userId, ct);

        public Task AdminAdjustAsync(Guid companyId, decimal delta, string reason, string userId, CancellationToken ct)
            => createCashAdjustmentUseCase.ExecuteAsync(companyId, delta, reason, userId, ct);

        public Task RebuildBalanceAsync(Guid companyId, CancellationToken ct)
            => rebuildCashBalanceUseCase.ExecuteAsync(companyId, ct);

        public Task<decimal> GetCurrentAsync(Guid companyId, CancellationToken ct)
            => getCashBalanceUseCase.ExecuteAsync(companyId, ct);

        public async Task<IReadOnlyList<CashLedger>> GetLedgerAsync(Guid companyId, DateTimeOffset? from, DateTimeOffset? to, int skip, int take, CancellationToken ct)
        {
            var entries = await getCashLedgerUseCase.ExecuteAsync(companyId, from, to, skip, take, ct);
            return entries.Select(entry => new CashLedger
            {
                Id = entry.Id,
                CompanyId = entry.CompanyId,
                RefType = entry.RefType,
                RefId = entry.RefId,
                OriginalPaymentId = entry.OriginalPaymentId,
                Delta = entry.Delta,
                Reason = entry.Reason,
                CreatedBy = entry.CreatedBy,
                CreatedAtUtc = entry.CreatedAtUtc
            }).ToList();
        }
    }
}
