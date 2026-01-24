using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Models.Identity;
using Shared.Models;

namespace Shared.DTOs
{
    public class Payment_Create_DTO
    {
        #region Data Linking
        [Required, NotNull, Key, Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid PaymentID { get; set; } = Guid.NewGuid();

        [Required, NotNull, Column(Order = 1)]
        public Guid TenantID { get; set; }

        /// <summary>
        /// Unique identifier for idempotency (unique per company).
        /// Generated client-side and reused on retry to prevent duplicate submissions.
        /// </summary>
        [Required, NotNull, Column(Order = 2)]
        public Guid RequestId { get; set; } = Guid.NewGuid();
        #endregion

        [Required, Display(Name = "Amount"), Range(0, double.MaxValue, ErrorMessage = "Amount must be non-negative")]
        public double Amount { get; set; }

        [AllowNull, Display(Name = "EntryType")]
        public required string EntryType { get; set; }// “Income” or “Outcome”

        [Required, NotNull]
        public Guid? PaymentMethodID { get; set; } // “Cash” or “Card”

        [Required, NotNull]
        public Guid? DocumentTypeID { get; set; }// “Invoice” or “Receipt”

        [AllowNull]
        public string? Description { get; set; }

        [Required, ForeignKey("UserID")]
        public Guid UserID { get; set; }

        public string EmployeeFullName { get; set; } = string.Empty;

        #region Deletititon
        [Required, NotNull, Display(Name = "Is Customer deleted ?")]
        public bool IsDeleted { get; set; } = false;

        [AllowNull, Column(Order = 801), Display(Name = "Who deleted this Customer ?")]
        public Guid? IsDeletedBy { get; set; }

        [AllowNull, Display(Name = "Why is Customer deleted ?")]
        public string? IsDeletedWhy { get; set; }

        [AllowNull, Display(Name = "Date deleted"), DataType(DataType.DateTime)]
        public DateTime? DateDeleted { get; set; }
        #endregion

        #region Audit & Tracking
        [AllowNull, Display(Name = "Created By")]
        public Guid? CreatedBy { get; set; }

        [Required, NotNull, Display(Name = "Created")]
        public DateTime DateIns { get; set; } = DateTime.UtcNow;

        [AllowNull, Display(Name = "Edited By")]
        public Guid? EditedBy { get; set; }

        [AllowNull, Display(Name = "Date edit")]
        public DateTime? DateEdit { get; set; }
        #endregion
    }
}
