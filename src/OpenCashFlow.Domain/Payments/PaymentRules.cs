namespace OpenCashFlow.Domain.Payments;

public static class PaymentRules
{
    public static decimal GetCashDelta(PaymentAmount amount, PaymentEntryType entryType)
    {
        if (entryType.IsInflow)
        {
            return amount.Amount;
        }

        if (entryType.IsOutflow)
        {
            return -amount.Amount;
        }

        throw new ArgumentException("Payment entry type must be Inflow or Outflow.", nameof(entryType));
    }
}

