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
    public class Payment_Detail_DTO
    {
        #region Data Linking
        [Required, NotNull, Key]
        public Guid PaymentID { get; set; } = Guid.NewGuid();

        [Required, NotNull]
        public Guid TenantID { get; set; }
        #endregion

        [Required, Display(Name = "Amount")]
        public double Amount { get; set; }

        [AllowNull, Display(Name = "EntryType")]
        public required string EntryType { get; set; }// “Income” or “Outcome”

        [Required, NotNull]
        public Guid? PaymentMethodID { get; set; } // “Cash” or “Card”

        public string PaymentMethodName { get; set; } = string.Empty;

        [Required, NotNull]
        public Guid? DocumentTypeID { get; set; }// “Invoice” or “Receipt”

        public string DocumentTypeName { get; set; } = string.Empty;

        [AllowNull, Display(Name = "Description")]
        public string? Description { get; set; }

        [Required, Column(Order = 15)]
        public Guid UserID { get; set; }

        public string EmployeeFullName { get; set; } = string.Empty;

        #region Deletititon
        [Required, NotNull, Display(Name = "Is Customer deleted ?")]
        public bool IsDeleted { get; set; } = false;

        [AllowNull, Display(Name = "Who deleted this Customer ?")]
        public Guid? IsDeletedBy { get; set; }

        [AllowNull, Display(Name = "Why is Customer deleted ?")]
        public string? IsDeletedWhy { get; set; }

        [AllowNull, Display(Name = "Date deleted")]
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
