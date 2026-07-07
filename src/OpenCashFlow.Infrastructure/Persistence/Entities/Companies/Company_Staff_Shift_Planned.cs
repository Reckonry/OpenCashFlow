using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace OpenCashFlow.Infrastructure.Persistence.Entities
{
    [Table(name: "Companies_Staff_Shifts_Planned")]
    [PrimaryKey(nameof(PlannedShiftID))]
    public class Company_Staff_Shift_Planned
    {
        #region Data Linking
        [Required, NotNull, Column(Order = 0), ForeignKey("TenantID")]
        public Guid TenantID { get; set; } // Collegamento all'azienda

        [Required, NotNull, Column(Order = 1), ForeignKey("UserID")]
        public Guid UserID { get; set; } // Collegamento al dipendente
        #endregion

        #region Shift Identification
        [Required, Key, Column(Order = 10), DefaultValue("NewID()")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid PlannedShiftID { get; set; } = Guid.NewGuid();
        #endregion

        #region Shift Details
        [Required(ErrorMessage = "Provide a shift start date"), NotNull, Column(Order = 20)]
        [Display(Name = "Planned Shift Start"), DataType(DataType.DateTime)]
        public DateTime PlannedShiftStart { get; set; }

        [AllowNull, Column(Order = 21)]
        [Display(Name = "Planned Shift End"), DataType(DataType.DateTime)]
        public DateTime? PlannedShiftEnd { get; set; }

        [AllowNull, Column(TypeName = "varchar(50)", Order = 22), Display(Name = "Shift Type")]
        public string? ShiftType { get; set; } // Es: Standard, Overtime

        [AllowNull, Column(TypeName = "varchar(256)", Order = 23), Display(Name = "Shift Location")]
        public string? ShiftLocation { get; set; } // Luogo del turno

        [AllowNull, Column(TypeName = "varchar(50)", Order = 24), Display(Name = "Shift Category")]
        public string? ShiftCategory { get; set; } // Es: Diurno, Notturno

        [AllowNull, Column(TypeName = "text", Order = 25), Display(Name = "Notes")]
        public string? Notes { get; set; } // Note generiche
        #endregion

        #region Shift Status
        [AllowNull, Column(TypeName = "varchar(50)", Order = 30), Display(Name = "Status")]
        public string? Status { get; set; } = "Planned"; // Es: Planned, Confirmed, Modified

        [AllowNull, Column(Order = 31), Display(Name = "Approved By"), ForeignKey("UserID")]
        public Guid? ApprovedBy { get; set; }

        [AllowNull, Column(Order = 32), Display(Name = "Approval Date"), DataType(DataType.DateTime)]
        public DateTime? ApprovalDate { get; set; }
        #endregion

        #region Deletititon
        [Required, NotNull, Column(Order = 802), Display(Name = "Is deleted ?"), DefaultValue("0")]
        public bool IsDeleted { get; set; } = false;

        [AllowNull, Column(Order = 803), Display(Name = "Who deleted this ?")]
        public Guid? IsDeletedBy { get; set; }

        [AllowNull, Column(TypeName = "text", Order = 804), Display(Name = "Why is deleted ?")]
        public string? IsDeletedWhy { get; set; }

        [AllowNull, Column(Order = 805), Display(Name = "Date deleted"), DataType(DataType.DateTime)]
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

        [AllowNull, Column(Order = 903)]
        [Display(Name = "Date edit"), DataType(DataType.DateTime)]
        public DateTime? DateEdit { get; set; }
        #endregion
    }
}
