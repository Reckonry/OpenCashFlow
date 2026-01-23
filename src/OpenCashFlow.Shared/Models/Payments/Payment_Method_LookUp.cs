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

namespace Shared.Models
{
    [Table("Payments_Methods_Lookups")]
    [PrimaryKey(nameof(PaymentMethodID))]
    public class Payment_Method_LookUps
    {
        [Required, NotNull, Key, Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid PaymentMethodID { get; set; } = Guid.NewGuid();

        #region Document Details
        [Required, Column(TypeName = "varchar(256)", Order = 10), Display(Name = "PaymentMethodName")]
        public required string PaymentMethodName { get; set; } = string.Empty; // e.g., "Invoice", "Receipt"

        [Required, Column(TypeName = "varchar(50)", Order = 11)]
        /// <summary>
        /// Descrizione del tipo di documento.
        /// </summary>
        public string? PaymentMethodDescription { get; set; }

        [AllowNull, Column(TypeName = "text", Order = 12)]
        /// <summary>
        /// Icon for the payment method.
        /// </summary>
        public string? PaymentMethodIcon { get; set; }
        #endregion



        [Required, Column(Order = 13), DefaultValue(true)]
        /// <summary>
        /// Indicates whether the payment method is visible.
        /// </summary>
        public bool Visible { get; set; } = true;

        [AllowNull, Column(Order = 14)]
        /// <summary>
        /// Display order for the payment method.
        /// </summary>
        public int DisplayOrder { get; set; } = 999;

        #region For custom values
        [AllowNull, Column(Order = 701), ForeignKey("TenantID")]
        public Guid? TenantID { get; set; }
        #endregion

        #region Deletititon
        [Required, NotNull, Column(Order = 800), Display(Name = "Is Customer deleted ?"), DefaultValue(false)]
        public bool IsDeleted { get; set; } = false;

        [AllowNull, Column(Order = 801), Display(Name = "Who deleted this Customer ?"), ForeignKey("UserID")]
        public Guid? IsDeletedBy { get; set; }

        [AllowNull, Column(TypeName = "text", Order = 802), Display(Name = "Why is Customer deleted ?")]
        public string? IsDeletedWhy { get; set; }

        [AllowNull, Column(Order = 803), Display(Name = "Date deleted"), DataType(DataType.DateTime)]
        public DateTime? DateDeleted { get; set; }
        #endregion

        #region Audit & Tracking
        [AllowNull, Column(Order = 900), Display(Name = "Created By"), ForeignKey("UserID")]
        public Guid? CreatedBy { get; set; }

        [Required, NotNull, Column(Order = 901), DefaultValue("now()")]
        [Display(Name = "Created"), DataType(DataType.DateTime)]
        public DateTime DateIns { get; set; } = DateTime.UtcNow;

        [AllowNull, Column(Order = 902), Display(Name = "Edited By"), ForeignKey("UserID")]
        public Guid? EditedBy { get; set; }

        [AllowNull, Column(Order = 903), Display(Name = "Date edit"), DataType(DataType.DateTime)]
        public DateTime? DateEdit { get; set; }
        #endregion
    }
}
