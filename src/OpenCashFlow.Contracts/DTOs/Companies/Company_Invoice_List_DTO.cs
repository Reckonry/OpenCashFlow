namespace OpenCashFlow.Contracts.DTOs.Companies;

public class Company_Invoice_List_DTO
{
    public Guid TenantID { get; set; }
    public Guid InvoiceID { get; set; }
    public required string CompanyName { get; set; }
    public required string BillingAddressLine1 { get; set; }
    public string? BillingAddressLine2 { get; set; }
    public required string BillingCity { get; set; }
    public required string BillingState { get; set; }
    public required string BillingPostalCode { get; set; }
    public required string BillingCountry { get; set; }
    public string? TaxIdentificationNumber { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public required string InvoiceNumber { get; set; }
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string Status { get; set; } = "UNPAID";
    public decimal TotalAmount { get; set; }
    public decimal TotalTaxes { get; set; }
    public decimal? AmountPaid { get; set; }
    public string? Notes { get; set; }
    public ICollection<CompanyInvoiceItemDto> Items { get; set; } = [];
    public string? IvoicePath { get; set; }
    public string PaymentMethod { get; set; } = "BANK_TRANSFER";
    public string? TransactionID { get; set; }
    public string? PaymentProcessor { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime DateIns { get; set; } = DateTime.UtcNow;
    public Guid? EditedBy { get; set; }
    public DateTime? DateEdit { get; set; }
}
