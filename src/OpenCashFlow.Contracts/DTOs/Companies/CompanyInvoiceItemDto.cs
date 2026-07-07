using System.ComponentModel.DataAnnotations;

namespace OpenCashFlow.Contracts.DTOs.Companies;

public class CompanyInvoiceItemDto
{
    public Guid TenantID { get; set; }
    public Guid InvoiceItemID { get; set; }
    public Guid InvoiceID { get; set; }
    public string? Description { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? TaxPercent { get; set; } = 0;
    public decimal? Subtotal => Quantity * UnitPrice;
    public decimal? TotalWithTax => Subtotal + (Subtotal * TaxPercent / 100);
}
