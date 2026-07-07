namespace OpenCashFlow.Application.Companies.Models;

public sealed class CompanyResult
{
    public Guid TenantID { get; set; }
    public required string CompanyName { get; set; }
    public string? SysCompanyName { get; set; }
    public long MaxUsers { get; set; }
    public string? Avatar { get; set; }
    public string? BusinessCategory { get; set; }
    public decimal? EstimatedAnnualRevenue { get; set; }
    public string? BusinessHours { get; set; }
    public string? Website { get; set; }
    public string? SocialLinks { get; set; }
    public decimal? InternalRating { get; set; }
    public int PriorityLevel { get; set; }
    public double? VATRates { get; set; }
    public string? VAT { get; set; }
    public string? SDI { get; set; }
    public string? NIN { get; set; }
    public string? AttorneyName { get; set; }
    public string? AttorneyMiddleName { get; set; }
    public string? AttorneySurname { get; set; }
    public string? IBAN { get; set; }
    public string? BIC { get; set; }
    public string? SWIFT { get; set; }
    public string? PreferredPaymentMethod { get; set; }
    public decimal? MonthlyExpenseLimit { get; set; }
    public double? BaseDiscountPercentage { get; set; }
    public DateTime StartingContract { get; set; }
    public DateTime EndingContract { get; set; }
    public string? LicenseType { get; set; }
    public bool GdprConsent { get; set; }
    public DateTime? GdprConsentDate { get; set; }
    public bool ContractAcepted { get; set; }
    public string? ContractVersion { get; set; }
    public DateTime? ContractAcceptedDate { get; set; }
    public string? DefaultCurrency { get; set; }
    public string? DefaultTimezone { get; set; }
    public string? DefaultLanguage { get; set; }
    public string? DefaultCountry { get; set; }
    public bool IsActive { get; set; }
    public Guid? StatusID { get; set; }
    public bool IsDeleted { get; set; }
    public Guid? IsDeletedBy { get; set; }
    public string? IsDeletedWhy { get; set; }
    public DateTime? DateDeleted { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime DateIns { get; set; }
    public Guid? EditedBy { get; set; }
    public DateTime? DateEdit { get; set; }
}

public sealed class CompanyInvoiceItemResult
{
    public Guid TenantID { get; set; }
    public Guid InvoiceItemID { get; set; }
    public Guid InvoiceID { get; set; }
    public string? Description { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? TaxPercent { get; set; }
}

public class CompanyInvoiceResult
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
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string Status { get; set; } = "UNPAID";
    public decimal TotalAmount { get; set; }
    public decimal TotalTaxes { get; set; }
    public decimal? AmountPaid { get; set; }
    public string? Notes { get; set; }
    public IReadOnlyList<CompanyInvoiceItemResult> Items { get; set; } = [];
    public string? IvoicePath { get; set; }
    public string PaymentMethod { get; set; } = "BANK_TRANSFER";
    public string? TransactionID { get; set; }
    public string? PaymentProcessor { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime DateIns { get; set; }
    public Guid? EditedBy { get; set; }
    public DateTime? DateEdit { get; set; }
}

public sealed class CompanyInvoiceListItem : CompanyInvoiceResult;

public sealed class CompanyInvoiceDetailResult : CompanyInvoiceResult;
