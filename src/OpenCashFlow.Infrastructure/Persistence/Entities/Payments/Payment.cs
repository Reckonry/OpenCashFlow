using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCashFlow.Infrastructure.Persistence.Entities.Identity;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace OpenCashFlow.Infrastructure.Persistence.Entities
{
[Table("Payments")]
[PrimaryKey(nameof(PaymentID))]
//[CheckConstraint("CK_Payments_Amount_NotNegative", "[Amount] >= 0")]
    public class Payment
    {
        #region Data Linking
        [Required, NotNull, Key, Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid PaymentID { get; set; } = Guid.NewGuid();

        [Required, NotNull, Column(Order = 1)]
        public Guid TenantID { get; set; }

        /// <summary>
        /// Unique identifier for idempotency (unique per company).
        /// Used to prevent duplicate payment submissions.
        /// </summary>
        [Required, NotNull, Column(Order = 2)]
        public Guid RequestId { get; set; } = Guid.NewGuid();
        #endregion

        [Required, Column(TypeName = "numeric(18,3)", Order = 10), Display(Name = "Amount")]
        [Range(0.01, double.MaxValue)]
        public double Amount { get; set; }
        
        [AllowNull, Column(TypeName = "varchar(256)", Order = 11), Display(Name = "EntryType")]
        public required string EntryType { get; set; }// “Income” or “Outcome”
       
        [Required, NotNull, Column(Order = 12)]
        public Guid PaymentMethodID { get; set; } // “Cash” or “Card”

        [ForeignKey(nameof(PaymentMethodID))]
        public virtual Payment_Method_LookUps? PaymentMethod { get; set; }

        [Required, NotNull, Column(Order = 13)]
        public Guid DocumentTypeID { get; set; }// “Invoice” or “Receipt”

        [ForeignKey(nameof(DocumentTypeID))]
        public virtual Payment_DocumentType_LookUp? DocumentType { get; set; }

        [AllowNull, Column(TypeName = "text", Order = 14), Display(Name = "Description")]
        public string? Description { get; set; }

        [Required, Column(Order = 15)]
        public Guid UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual AspNetUser? User { get; set; }

        #region Deletititon
        [Required, NotNull, Column(Order = 800), Display(Name = "Is Customer deleted ?"), DefaultValue(false)]
        public bool IsDeleted { get; set; } = false;

        [AllowNull, Column(Order = 801), Display(Name = "Who deleted this Customer ?")]
        public Guid? IsDeletedBy { get; set; }

        [AllowNull, Column(TypeName = "text", Order = 802), Display(Name = "Why is Customer deleted ?")]
        public string? IsDeletedWhy { get; set; }

        [AllowNull, Column(Order = 803), Display(Name = "Date deleted"), DataType(DataType.DateTime)]
        public DateTime? DateDeleted { get; set; }
        #endregion

        #region Audit & Tracking
        [AllowNull, Column(Order = 900), Display(Name = "Created By")]
        public Guid? CreatedBy { get; set; }

        [Required, NotNull, Column(Order = 901), DefaultValue("now()")]
        [Display(Name = "Created"), DataType(DataType.DateTime)]
        public DateTime DateIns { get; set; } = DateTime.UtcNow;

        [AllowNull, Column(Order = 902), Display(Name = "Edited By")]
        public Guid? EditedBy { get; set; }

        [AllowNull, Column(Order = 903), Display(Name = "Date edit"), DataType(DataType.DateTime)]
        public DateTime? DateEdit { get; set; }
        #endregion

    }

}
