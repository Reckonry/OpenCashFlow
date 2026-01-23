using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Shared.Models
{
    [Table(name: "Companies_Staff_LeaveRequests")]
    [PrimaryKey(nameof(LeaveRequestID))]
    public class Company_Staff_LeaveRequest
    {
        #region Data Linking
        [Required, NotNull, Column(Order = 1), ForeignKey("TenantID")]
        public Guid TenantID { get; set; } // Collegamento all'azienda

        [Required, NotNull, Column(Order = 2), ForeignKey("UserID")]
        public Guid UserID { get; set; } // Collegamento al dipendente
        #endregion

        #region Leave Request Identification
        [Required, Key, Column(Order = 0), DefaultValue("NewID()")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid LeaveRequestID { get; set; } = Guid.NewGuid();
        #endregion

        #region Leave Details
        [Required, NotNull, Column(Order = 3), Display(Name = "Start Date"), DataType(DataType.DateTime)]
        public DateTime LeaveStartDate { get; set; } // Data di inizio del permesso/ferie

        [Required, NotNull, Column(Order = 4), Display(Name = "End Date"), DataType(DataType.DateTime)]
        public DateTime LeaveEndDate { get; set; } // Data di fine del permesso/ferie

        [AllowNull, Column(TypeName = "varchar(50)", Order = 5), Display(Name = "Leave Type")]
        public string? LeaveType { get; set; } // Es: Ferie, Permesso, Malattia

        [AllowNull, Column(TypeName = "text", Order = 6), Display(Name = "Reason")]
        public string? Reason { get; set; } // Motivazione della richiesta
        #endregion

        #region Approval and Status
        [AllowNull, Column(TypeName = "varchar(50)", Order = 7), Display(Name = "Status")]
        public string? Status { get; set; } = "Pending"; // Es: Pending, Approved, Rejected

        [AllowNull, Column(Order = 8), Display(Name = "Approved By"), ForeignKey("UserID")]
        public Guid? ApprovedBy { get; set; } // ID del responsabile approvante

        [AllowNull, Column(Order = 9), Display(Name = "Approval Date"), DataType(DataType.DateTime)]
        public DateTime? ApprovalDate { get; set; } // Data di approvazione

        [AllowNull, Column(TypeName = "varchar(256)", Order = 10), Display(Name = "External System Reference")]
        public string? ExternalSystemReference { get; set; } // Riferimenti a sistemi esterni

        [AllowNull, Column(TypeName = "varchar(50)", Order = 11), Display(Name = "Sync Status")]
        public string? SyncStatus { get; set; } // Stato di sincronizzazione
        #endregion

        #region Notes
        [AllowNull, Column(TypeName = "text", Order = 12), Display(Name = "Notes")]
        public string? Notes { get; set; } // Note aggiuntive
        #endregion

        #region Deletion
        [Required, NotNull, Column(Order = 801), Display(Name = "Is Deleted?"), DefaultValue(false)]
        public bool IsDeleted { get; set; } = false;

        [AllowNull, Column(Order = 802), Display(Name = "Who Deleted This?"), ForeignKey("UserID")]
        public Guid? IsDeletedBy { get; set; }

        [AllowNull, Column(TypeName = "text", Order = 803), Display(Name = "Why Is Deleted?")]
        public string? IsDeletedWhy { get; set; }

        [AllowNull, Column(Order = 805), Display(Name = "Date deleted"), DataType(DataType.DateTime)]
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

        [AllowNull, Column(Order = 903)]
        [Display(Name = "Date Edit"), DataType(DataType.DateTime)]
        public DateTime? DateEdit { get; set; }
        #endregion
    }
}
