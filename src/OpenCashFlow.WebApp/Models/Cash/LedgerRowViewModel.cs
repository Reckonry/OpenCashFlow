namespace OpenCashFlow.WebApp.Models.Cash;

public class LedgerRowViewModel
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string RefType { get; set; } = default!;
    public Guid RefId { get; set; }
    public Guid? OriginalPaymentId { get; set; }
    public decimal Delta { get; set; }
    public string? Reason { get; set; }
    public string CreatedBy { get; set; } = default!;
    public DateTimeOffset CreatedAtUtc { get; set; }
}
