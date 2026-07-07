namespace OpenCashFlow.Contracts.Cash;

public class CashLedger
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public string RefType { get; set; } = default!;
    public Guid RefId { get; set; }
    public Guid? OriginalPaymentId { get; set; }
    public decimal Delta { get; set; }
    public string? Reason { get; set; }
    public string CreatedBy { get; set; } = default!;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
