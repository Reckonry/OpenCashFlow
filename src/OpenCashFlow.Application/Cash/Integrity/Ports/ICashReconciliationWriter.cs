using OpenCashFlow.Application.Cash.Integrity.Commands;
using OpenCashFlow.Application.Cash.Integrity.Models;

namespace OpenCashFlow.Application.Cash.Integrity.Ports;

public interface ICashReconciliationWriter
{
    Task<CashReconciliationResult?> ReconcileAsync(ReconcileCashSessionCommand command, CancellationToken cancellationToken = default);

    Task<CashDiscrepancyResult?> ExplainDiscrepancyAsync(ExplainCashDiscrepancyCommand command, CancellationToken cancellationToken = default);
}
