namespace OpenCashFlow.Domain.Common;

public readonly record struct TenantId
{
    public TenantId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("TenantId cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static TenantId New() => new(Guid.NewGuid());

    public static TenantId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}

