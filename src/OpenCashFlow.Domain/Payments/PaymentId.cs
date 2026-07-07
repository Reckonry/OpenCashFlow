namespace OpenCashFlow.Domain.Payments;

public readonly record struct PaymentId
{
    public PaymentId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("PaymentId cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static PaymentId New() => new(Guid.NewGuid());

    public static PaymentId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}

