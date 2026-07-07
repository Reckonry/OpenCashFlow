namespace OpenCashFlow.Domain.Payments;

public readonly record struct PaymentEntryType
{
    public const string InflowValue = "Inflow";
    public const string OutflowValue = "Outflow";

    private PaymentEntryType(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public bool IsInflow => Value == InflowValue;

    public bool IsOutflow => Value == OutflowValue;

    public static PaymentEntryType Inflow => new(InflowValue);

    public static PaymentEntryType Outflow => new(OutflowValue);

    public static PaymentEntryType From(string value)
    {
        return Normalize(value) switch
        {
            "INFLOW" or "INCOME" or "IN" or "ENTRATA" => Inflow,
            "OUTFLOW" or "EXPENSE" or "OUT" or "USCITA" => Outflow,
            _ => throw new ArgumentException("Payment entry type must be Inflow or Outflow.", nameof(value))
        };
    }

    public override string ToString() => Value;

    private static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Payment entry type is required.", nameof(value));
        }

        return value.Trim().ToUpperInvariant();
    }
}

