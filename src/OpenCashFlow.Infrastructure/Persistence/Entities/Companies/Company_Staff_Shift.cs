using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCashFlow.Infrastructure.Persistence.Entities
{
    [Table(name: "Companies_Staff_Shifts")]
    [PrimaryKey(nameof(ShiftID))]
    public class Company_Staff_Shift
    {
        #region Data Linking
        [Required, NotNull, Column(Order = 1), ForeignKey("TenantID")]
        public Guid TenantID { get; set; } // Collegamento all'azienda

        [Required(ErrorMessage = "Choose an employee"), NotNull, Column(Order = 2), Display(Name = "Employee"), ForeignKey("UserID")]
        public Guid UserID { get; set; } // Collegamento con il dipendente
        #endregion

        #region Shift Identification
        [Required, Key, Column(Order = 0), DefaultValue("NewID()")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ShiftID { get; set; } = Guid.NewGuid();
        #endregion

        #region Shift Details
        [Required(ErrorMessage = "Provide a shift start date"), NotNull, Column(Order = 3)]
        [Display(Name = "Shift Start"), DataType(DataType.DateTime)]
        public DateTime ShiftStart { get; set; } = DateTime.UtcNow;

        [AllowNull, Column(Order = 4)]
        [Display(Name = "Shift End"), DataType(DataType.DateTime)]
        public DateTime? ShiftEnd { get; set; }

        [AllowNull, Column(TypeName = "varchar(50)", Order = 5), Display(Name = "Shift Type")]
        public string? ShiftType { get; set; } // Es: Standard, Overtime

        [AllowNull, Column(TypeName = "varchar(256)", Order = 6), Display(Name = "Shift Location")]
        public string? ShiftLocation { get; set; } // Luogo del turno

        [AllowNull, Column(TypeName = "varchar(50)", Order = 7), Display(Name = "Shift Category")]
        public string? ShiftCategory { get; set; } // Es: Diurno, Notturno

        [AllowNull, Column(TypeName = "numeric(18,2)", Order = 8), Display(Name = "Break Duration")]
        public double? BreakDuration { get; set; } // Durata delle pause in minuti o ore
        #endregion

        #region Overtime and Bonuses
        [Required, NotNull, Column(Order = 9), Display(Name = "Is Overtime"), DefaultValue(false)]
        public bool IsOvertime { get; set; } = false;

        [AllowNull, Column(TypeName = "numeric(18,2)", Order = 10), Display(Name = "Overtime Hours")]
        public double? OvertimeHours { get; set; }

        [AllowNull, Column(TypeName = "decimal(18,2)", Order = 11), Display(Name = "Special Allowance")]
        public decimal? SpecialAllowance { get; set; }

        [Required, NotNull, Column(Order = 12), Display(Name = "Is Night Shift"), DefaultValue(false)]
        public bool IsNightShift { get; set; } = false;
        #endregion

        #region Monitoring and Approval
        [AllowNull, Column(Order = 20), Display(Name = "Approved By"), ForeignKey("UserID")]
        public Guid? ApprovedBy { get; set; }

        [AllowNull, Column(Order = 21), Display(Name = "Approval Date"), DataType(DataType.DateTime)]
        public DateTime? ApprovalDate { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 22), Display(Name = "External System Reference")]
        public string? ExternalSystemReference { get; set; } // Collegamenti a sistemi esterni

        [AllowNull, Column(TypeName = "varchar(50)", Order = 23), Display(Name = "Sync Status")]
        public string? SyncStatus { get; set; } // Stato di sincronizzazione
        #endregion

        #region KPI and Tracking
        [AllowNull, Column(TypeName = "decimal(18,3)", Order = 30), Display(Name = "Shift Efficiency")]
        public decimal? ShiftEfficiency { get; set; } // KPI del turno

        [AllowNull, Column(Order = 31), Display(Name = "Completed Tasks")]
        public int? CompletedTasks { get; set; } // Number of completed tasks
        #endregion

        #region Notes
        [AllowNull, Column(TypeName = "text", Order = 40), Display(Name = "Notes")]
        public string? Notes { get; set; } // Note generiche
        #endregion

        #region Deletion
        [Required, NotNull, Column(Order = 801), Display(Name = "Is deleted?"), DefaultValue(false)]
        public bool IsDeleted { get; set; } = false;

        [AllowNull, Column(Order = 802), Display(Name = "Who deleted this?"), ForeignKey("UserID")]
        public Guid? IsDeletedBy { get; set; }

        [AllowNull, Column(TypeName = "text", Order = 803), Display(Name = "Why is deleted?")]
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
