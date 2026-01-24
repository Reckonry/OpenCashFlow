using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace Shared.Models.DTOs
{
    public class Payment_Method_List_DTO
    {
        [Required, NotNull, Key]
        public Guid PaymentMethodID { get; set; }

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

        #region Deletititon
        [Required, NotNull, Display(Name = "Is Customer deleted ?")]
        public bool IsDeleted { get; set; } = false;

        [AllowNull, Display(Name = "Who deleted this Customer ?")]
        public Guid? IsDeletedBy { get; set; }

        [AllowNull, Display(Name = "Why is Customer deleted ?")]
        public string? IsDeletedWhy { get; set; }

        [AllowNull, Display(Name = "Date deleted"), DataType(DataType.DateTime)]
        public DateTime? DateDeleted { get; set; }
        #endregion

        #region Audit & Tracking
        [AllowNull, Display(Name = "Created By")]
        public Guid? CreatedBy { get; set; }

        [Required, NotNull, Display(Name = "Created"), DataType(DataType.DateTime)]
        public DateTime DateIns { get; set; }

        [AllowNull]
        public Guid? EditedBy { get; set; }

        [AllowNull]
        public DateTime? DateEdit { get; set; }
        #endregion
    }
}
