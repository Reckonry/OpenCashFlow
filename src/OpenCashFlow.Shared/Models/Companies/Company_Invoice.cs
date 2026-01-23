using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Shared.Models
{
    [Table(name: "Companies_Invoices")]
    [PrimaryKey(nameof(InvoiceID))]
    public class Company_Invoice
    {
        #region Data Linking
        [Required, NotNull, Column(Order = 0), ForeignKey("TenantID")]
        public Guid TenantID { get; set; } // Customer company the invoice is addressed to
        #endregion

        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.Identity),
            DefaultValue("NEWID()"), Column(Order = 10)]
        public Guid InvoiceID { get; set; } // Unique invoice ID

        #region Company Information
        [Required, Column(TypeName = "varchar(256)", Order = 20)]
        /// <summary>
        /// Customer company name.
        /// </summary>
        public required string CompanyName { get; set; }

        [Required, Column(TypeName = "varchar(256)", Order = 21)]
        /// <summary>
        /// Billing address - street.
        /// </summary>
        public required string BillingAddressLine1 { get; set; }

        [Column(TypeName = "varchar(256)", Order = 22)]
        /// <summary>
        /// Billing address - additional details (e.g., floor, office).
        /// </summary>
        public string? BillingAddressLine2 { get; set; }

        [Required, Column(TypeName = "varchar(100)", Order = 23)]
        /// <summary>
        /// City.
        /// </summary>
        public required string BillingCity { get; set; }

        [Required, Column(TypeName = "varchar(100)", Order = 24)]
        /// <summary>
        /// State/Province/Region.
        /// </summary>
        public required string BillingState { get; set; }

        [Required, Column(TypeName = "varchar(20)", Order = 25)]
        /// <summary>
        /// Postal code.
        /// </summary>
        public required string BillingPostalCode { get; set; }

        [Required, Column(TypeName = "varchar(100)", Order = 26)]
        /// <summary>
        /// Country.
        /// </summary>
        public required string BillingCountry { get; set; }

        [Column(TypeName = "varchar(50)", Order = 27)]
        /// <summary>
        /// Company tax ID or VAT number.
        /// </summary>
        public string? TaxIdentificationNumber { get; set; }

        [Column(TypeName = "varchar(256)", Order = 28)]
        /// <summary>
        /// Company contact name.
        /// </summary>
        public string? ContactName { get; set; }

        [Column(TypeName = "varchar(100)", Order = 29)]
        /// <summary>
        /// Company contact email.
        /// </summary>
        public string? ContactEmail { get; set; }

        [Column(TypeName = "varchar(20)", Order = 30)]
        /// <summary>
        /// Contact phone number.
        /// </summary>
        public string? ContactPhone { get; set; }
        #endregion

        #region Invoice Details
        [Required, Column(TypeName = "varchar(256)", Order = 40)]
        /// <summary>
        /// Invoice number (e.g., "INV-2025-001").
        /// </summary>
        public required string InvoiceNumber { get; set; }

        [Required, Column(Order = 41), DataType(DataType.DateTime)]
        /// <summary>
        /// Invoice issue date.
        /// </summary>
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;

        [Required, Column(Order = 42), DataType(DataType.DateTime)]
        /// <summary>
        /// Payment due date.
        /// </summary>
        public DateTime DueDate { get; set; }

        [Column(Order = 43), DataType(DataType.DateTime)]
        /// <summary>
        /// Payment date (if completed).
        /// </summary>
        public DateTime? PaymentDate { get; set; }

        [Required, Column(TypeName = "varchar(256)", Order = 44)]
        /// <summary>
        /// Invoice status (e.g., "PAID", "UNPAID", "OVERDUE").
        /// </summary>
        public string Status { get; set; } = "UNPAID";

        [Required, Column(TypeName = "numeric(18,2)", Order = 45)]
        /// <summary>
        /// Invoice total amount.
        /// </summary>
        public decimal TotalAmount { get; set; }

        [Required, Column(TypeName = "numeric(18,2)", Order = 46)]
        /// <summary>
        /// Total taxes applied.
        /// </summary>
        public decimal TotalTaxes { get; set; }

        [Column(TypeName = "numeric(18,2)", Order = 47)]
        /// <summary>
        /// Amount paid.
        /// </summary>
        public decimal? AmountPaid { get; set; }

        [Column(Order = 48)]
        /// <summary>
        /// Additional details or notes about the invoice.
        /// </summary>
        public string? Notes { get; set; }
        #endregion

        [ForeignKey(nameof(InvoiceID))]
        public virtual ICollection<Company_Invoice_Item> Items { get; set; } = [];

        [Required, Column(TypeName = "text", Order = 50)]
        /// <summary>
        /// Invoice path
        /// </summary>
        public string? IvoicePath { get; set; }

        #region Payment Information
        [Required, Column(TypeName = "varchar(150)", Order = 60)]
        /// <summary>
        /// Payment method used (e.g., "CREDIT_CARD", "BANK_TRANSFER").
        /// </summary>
        public string PaymentMethod { get; set; } = "BANK_TRANSFER";

        [AllowNull, Column(TypeName = "varchar(150)", Order = 61)]
        /// <summary>
        /// Transaction ID (returned by the payment processor).
        /// </summary>
        public string? TransactionID { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 62)]
        /// <summary>
        /// Payment processor name (e.g., "Stripe", "PayPal").
        /// </summary>
        public string? PaymentProcessor { get; set; }
        #endregion

        #region Audit & Tracking
        [AllowNull, Column(Order = 900), Display(Name = "Created By")]
        public Guid? CreatedBy { get; set; }

        [Required, NotNull, Column(Order = 901), DefaultValue("GETDATE()")]
        [Display(Name = "Created"), DataType(DataType.DateTime)]
        public DateTime DateIns { get; set; } = DateTime.UtcNow;

        [AllowNull, Column(Order = 902), Display(Name = "Edited By")]
        public Guid? EditedBy { get; set; }

        [AllowNull, Column(Order = 903)]
        [Display(Name = "Date Edited"), DataType(DataType.DateTime)]
        public DateTime? DateEdit { get; set; }
        #endregion


    }
}
