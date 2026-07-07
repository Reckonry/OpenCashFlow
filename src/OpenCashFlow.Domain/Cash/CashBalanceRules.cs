using OpenCashFlow.Domain.Common;

namespace OpenCashFlow.Domain.Cash;

public static class CashBalanceRules
{
    public static Money Apply(Money currentBalance, Money amount, CashLedgerEntryType entryType)
    {
        if (amount.Amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount.Amount, "Cash ledger amount cannot be negative.");
        }

        if (entryType.IsInflow)
        {
            return currentBalance.Add(amount);
        }

        if (entryType.IsOutflow)
        {
            return currentBalance.Subtract(amount);
        }

        throw new ArgumentException("Cash ledger entry type must be Inflow or Outflow.", nameof(entryType));
    }
}

