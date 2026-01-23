using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Models;

namespace Shared.DTOs.Companies
{
    public class Company_Invoices_Detail_DTO
    {
        #region Data Linking
        public Guid TenantID { get; set; } // Customer company the invoice is addressed to
        #endregion

        public Guid InvoiceID { get; set; } // Unique invoice ID

        #region Company Information
        [Required]
        /// <summary>
        /// Customer company name.
        /// </summary>
        public required string CompanyName { get; set; }

        /// <summary>
        /// Billing address - street.
        /// </summary>
        public required string BillingAddressLine1 { get; set; }

        /// <summary>
        /// Billing address - additional details (e.g., floor, office).
        /// </summary>
        public string? BillingAddressLine2 { get; set; }

        /// <summary>
        /// City.
        /// </summary>
        public required string BillingCity { get; set; }

        /// <summary>
        /// State/Province/Region.
        /// </summary>
        public required string BillingState { get; set; }

        /// <summary>
        /// Postal code.
        /// </summary>
        public required string BillingPostalCode { get; set; }

        /// <summary>
        /// Country.
        /// </summary>
        public required string BillingCountry { get; set; }

        /// <summary>
        /// Company tax ID or VAT number.
        /// </summary>
        public string? TaxIdentificationNumber { get; set; }

        /// <summary>
        /// Company contact name.
        /// </summary>
        public string? ContactName { get; set; }

        /// <summary>
        /// Company contact email.
        /// </summary>
        public string? ContactEmail { get; set; }

        /// <summary>
        /// Contact phone number.
        /// </summary>
        public string? ContactPhone { get; set; }
        #endregion

        #region Invoice Details
        /// <summary>
        /// Invoice number (e.g., "INV-2025-001").
        /// </summary>
        public required string InvoiceNumber { get; set; }

        /// <summary>
        /// Invoice issue date.
        /// </summary>
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Payment due date.
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Payment date (if completed).
        /// </summary>
        public DateTime? PaymentDate { get; set; }

        /// <summary>
        /// Invoice status (e.g., "PAID", "UNPAID", "OVERDUE").
        /// </summary>
        public string Status { get; set; } = "UNPAID";

        /// <summary>
        /// Invoice total amount.
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Total taxes applied.
        /// </summary>
        public decimal TotalTaxes { get; set; }

        /// <summary>
        /// Amount paid.
        /// </summary>
        public decimal? AmountPaid { get; set; }

        /// <summary>
        /// Additional details or notes about the invoice.
        /// </summary>
        public string? Notes { get; set; }
        #endregion

        public IEnumerable<Company_Invoice_Item> Items { get; set; } = [];

        /// <summary>
        /// Invoice path
        /// </summary>
        public string? IvoicePath { get; set; }

        #region Payment Information
        /// <summary>
        /// Payment method used (e.g., "CREDIT_CARD", "BANK_TRANSFER").
        /// </summary>
        public string PaymentMethod { get; set; } = "BANK_TRANSFER";

        /// <summary>
        /// Transaction ID (returned by the payment processor).
        /// </summary>
        public string? TransactionID { get; set; }

        /// <summary>
        /// Payment processor name (e.g., "Stripe", "PayPal").
        /// </summary>
        public string? PaymentProcessor { get; set; }
        #endregion

        #region Audit & Tracking
        public Guid? CreatedBy { get; set; }

        public DateTime DateIns { get; set; } = DateTime.UtcNow;

        public Guid? EditedBy { get; set; }

        public DateTime? DateEdit { get; set; }
        #endregion
    }
}
