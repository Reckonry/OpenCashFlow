using OpenCashFlow.Domain.Common;

namespace OpenCashFlow.Domain.Cash;

public sealed record CashReconciliation(
    Guid CashReconciliationId,
    TenantId TenantId,
    Guid CashSessionId,
    Guid CashAccountId,
    Money ExpectedBalance,
    Money ActualBalance,
    Money Discrepancy,
    CashReconciliationStatus Status,
    UserId ReconciledByUserId,
    DateTimeOffset ReconciledAtUtc)
{
    public static CashReconciliation Create(
        Guid cashReconciliationId,
        TenantId tenantId,
        Guid cashSessionId,
        Guid cashAccountId,
        Money expectedBalance,
        Money actualBalance,
        UserId reconciledByUserId,
        DateTimeOffset reconciledAtUtc)
    {
        if (cashReconciliationId == Guid.Empty)
        {
            throw new ArgumentException("Cash reconciliation id is required.", nameof(cashReconciliationId));
        }

        if (cashSessionId == Guid.Empty)
        {
            throw new ArgumentException("Cash session id is required.", nameof(cashSessionId));
        }

        if (cashAccountId == Guid.Empty)
        {
            throw new ArgumentException("Cash account id is required.", nameof(cashAccountId));
        }

        var discrepancy = actualBalance.Subtract(expectedBalance);
        var status = discrepancy.Amount == 0m
            ? CashReconciliationStatus.Balanced
            : CashReconciliationStatus.Discrepant;

        return new CashReconciliation(
            cashReconciliationId,
            tenantId,
            cashSessionId,
            cashAccountId,
            expectedBalance,
            actualBalance,
            discrepancy,
            status,
            reconciledByUserId,
            reconciledAtUtc);
    }

    public CashReconciliation MarkExplained(CashDiscrepancy discrepancy)
    {
        if (Status == CashReconciliationStatus.Balanced)
        {
            throw new InvalidOperationException("Balanced reconciliation cannot be explained as discrepant.");
        }

        if (discrepancy.CashReconciliationId != CashReconciliationId)
        {
            throw new InvalidOperationException("Discrepancy belongs to another reconciliation.");
        }

        return this with { Status = CashReconciliationStatus.Explained };
    }

    public CashReconciliation MarkResolved()
    {
        if (Status == CashReconciliationStatus.Balanced)
        {
            throw new InvalidOperationException("Balanced reconciliation is already resolved by balance.");
        }

        return this with { Status = CashReconciliationStatus.Resolved };
    }
}
