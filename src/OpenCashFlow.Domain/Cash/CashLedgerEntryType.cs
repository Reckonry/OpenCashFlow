namespace OpenCashFlow.Domain.Cash;

public readonly record struct CashLedgerEntryType
{
    public const string InflowValue = "Inflow";
    public const string OutflowValue = "Outflow";

    private CashLedgerEntryType(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public bool IsInflow => Value == InflowValue;

    public bool IsOutflow => Value == OutflowValue;

    public static CashLedgerEntryType Inflow => new(InflowValue);

    public static CashLedgerEntryType Outflow => new(OutflowValue);

    public static CashLedgerEntryType From(string value)
    {
        return Normalize(value) switch
        {
            "INFLOW" or "INCOME" or "IN" or "ENTRATA" => Inflow,
            "OUTFLOW" or "EXPENSE" or "OUT" or "USCITA" => Outflow,
            _ => throw new ArgumentException("Cash ledger entry type must be Inflow or Outflow.", nameof(value))
        };
    }

    public override string ToString() => Value;

    private static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Cash ledger entry type is required.", nameof(value));
        }

        return value.Trim().ToUpperInvariant();
    }
}

