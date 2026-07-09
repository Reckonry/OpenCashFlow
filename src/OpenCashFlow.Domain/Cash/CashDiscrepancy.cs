using OpenCashFlow.Domain.Common;

namespace OpenCashFlow.Domain.Cash;

public sealed record CashDiscrepancy(
    Guid CashDiscrepancyId,
    Guid CashReconciliationId,
    Money Amount,
    string Category,
    string Explanation,
    UserId CreatedByUserId,
    DateTimeOffset CreatedAtUtc)
{
    public static CashDiscrepancy Explain(
        Guid cashDiscrepancyId,
        Guid cashReconciliationId,
        Money amount,
        string category,
        string explanation,
        UserId createdByUserId,
        DateTimeOffset createdAtUtc)
    {
        if (cashDiscrepancyId == Guid.Empty)
        {
            throw new ArgumentException("Cash discrepancy id is required.", nameof(cashDiscrepancyId));
        }

        if (cashReconciliationId == Guid.Empty)
        {
            throw new ArgumentException("Cash reconciliation id is required.", nameof(cashReconciliationId));
        }

        if (amount.Amount == 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount.Amount, "Cash discrepancy amount cannot be zero.");
        }

        if (string.IsNullOrWhiteSpace(category))
        {
            throw new ArgumentException("Cash discrepancy category is required.", nameof(category));
        }

        if (string.IsNullOrWhiteSpace(explanation))
        {
            throw new ArgumentException("Cash discrepancy explanation is required.", nameof(explanation));
        }

        return new CashDiscrepancy(
            cashDiscrepancyId,
            cashReconciliationId,
            amount,
            category.Trim(),
            explanation.Trim(),
            createdByUserId,
            createdAtUtc);
    }
}
