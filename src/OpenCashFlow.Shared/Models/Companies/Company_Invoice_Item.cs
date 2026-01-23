using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Shared.Models
{
    [Table("Companies_Invoices_Items")]
    public class Company_Invoice_Item
    {
        #region Data Linking
        [Required, NotNull, Column(Order = 0), ForeignKey("TenantID")]
        public Guid TenantID { get; set; } // Customer company the invoice is addressed to

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid InvoiceItemID { get; set; }

        [Required, ForeignKey("InvoiceID")]
        public Guid InvoiceID { get; set; }
        #endregion

        [Required, Column(TypeName = "text")]
        /// <summary>
        /// Description of the billed item or service.
        /// </summary>
        public string? Description { get; set; }

        [Required, Column(TypeName = "numeric(18,2)")]
        /// <summary>
        /// Billed quantity.
        /// </summary>
        public decimal? Quantity { get; set; }

        [Required, Column(TypeName = "numeric(18,2)")]
        /// <summary>
        /// Unit price.
        /// </summary>
        public decimal? UnitPrice { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        /// <summary>
        /// Tax percentage for this line item (e.g., 22.00).
        /// </summary>
        public decimal? TaxPercent { get; set; } = 0;

        [Required, Column(TypeName = "numeric(18,2)")]
        /// <summary>
        /// Line total (quantity * unit price).
        /// </summary>
        public decimal? Subtotal => Quantity * UnitPrice;

        [NotMapped]
        /// <summary>
        /// Total calculation including taxes.
        /// </summary>
        public decimal? TotalWithTax => Subtotal + (Subtotal * TaxPercent / 100);
    }
}
