using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace OpenCashFlow.Contracts.DTOs.Payments
{
    public class Payment_Method_Create_DTO
    {
        [Required, NotNull, Key]
        public Guid PaymentMethodID { get; set; } = Guid.NewGuid();

        #region Document Details
        [Required, Display(Name = "PaymentMethodName")]
        public required string PaymentMethodName { get; set; } = string.Empty; // e.g., "Invoice", "Receipt"

        [Required]
        /// <summary>
        /// Payment method description.
        /// </summary>
        public string? PaymentMethodDescription { get; set; }

        [AllowNull]
        /// <summary>
        /// Icon for the payment method.
        /// </summary>
        public string? PaymentMethodIcon { get; set; }
        #endregion



        [Required]
        /// <summary>
        /// Indicates whether the payment method is visible.
        /// </summary>
        public bool Visible { get; set; } = true;

        [AllowNull]
        /// <summary>
        /// Display order for the payment method.
        /// </summary>
        public int DisplayOrder { get; set; } = 999;

        #region For custom values
        [AllowNull]
        public Guid? TenantID { get; set; }
        #endregion

        #region Audit & Tracking
        [AllowNull, Display(Name = "Created By")]
        public Guid? CreatedBy { get; set; }

        [Required, NotNull, Display(Name = "Created"), DataType(DataType.DateTime)]
        public DateTime DateIns { get; set; } = DateTime.UtcNow;
        #endregion
    }
}
