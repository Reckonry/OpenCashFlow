using System;

namespace Shared.DTOs.Billing
{
    public class BillingInvoice_DTO
    {
        public required string InvoiceId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal AmountDue { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountRemaining { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime? CreatedUtc { get; set; }
        public DateTime? DueDateUtc { get; set; }
        public string? HostedInvoiceUrl { get; set; }
        public string? InvoicePdf { get; set; }
    }
}
