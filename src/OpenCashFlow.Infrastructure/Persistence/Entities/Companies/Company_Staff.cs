using Microsoft.EntityFrameworkCore;
using OpenCashFlow.Infrastructure.Persistence.Entities.Identity;
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
    [Table(name: "Companies_Staff")]
    [PrimaryKey(nameof(UserID))]
    public class Company_Staff
    {
        #region Data Linking
        [Required, NotNull, Column(Order = 0), ForeignKey("TenantID")]
        public Guid TenantID { get; set; } // Collegamento all'azienda
        #endregion

        #region Employee Identification
        [Required, NotNull, Key, Column(Order = 10), ForeignKey("UserID")]
        public Guid UserID { get; set; } // Collegamento con AspNetUsers
        #endregion

        #region Operational Details
        [AllowNull, Column(TypeName = "numeric(18,3)", Order = 20), Display(Name = "Employee Cost per Hour")]
        public double? TimeCost { get; set; } // Costo orario del dipendente

        [AllowNull, Column(TypeName = "varchar(256)", Order = 21), Display(Name = "Badge ID")]
        public string? BadgeID { get; set; } // ID del badge per accesso

        [Required, NotNull, Column(Order = 22), Display(Name = "Is User Out of Reports?"), DefaultValue(false)]
        public bool OutOfReports { get; set; } = false; // Esclusione dai report

        [Required, NotNull, Column(Order = 23), Display(Name = "Is User Required to Check In?"), DefaultValue(true)]
        public bool RequireShiftCheckIn { get; set; } = true; // Obbligo di timbratura

        [AllowNull, Column(Order = 24), Display(Name = "Last Check In"), DataType(DataType.DateTime)]
        public DateTime? LastCheckIn { get; set; } // Ultimo check-in

        [AllowNull, Column(Order = 25), Display(Name = "Last Check Out"), DataType(DataType.DateTime)]
        public DateTime? LastCheckOut { get; set; } // Ultimo check-out
        #endregion

        #region Personal and Role Details
        [AllowNull, Column(TypeName = "varchar(50)", Order = 30), Display(Name = "Role")]
        public string? Role { get; set; } // Ruolo del dipendente (Es: Manager, Tecnico, etc.)

        [AllowNull, Column(TypeName = "varchar(256)", Order = 31), Display(Name = "Department")]
        public string? Department { get; set; } // Dipartimento di appartenenza

        [AllowNull, Column(TypeName = "varchar(50)", Order = 32), Display(Name = "Work Location")]
        public string? WorkLocation { get; set; } // Sede di lavoro
        #endregion

        #region Employment Details
        [AllowNull, Column(Order = 40), Display(Name = "Contract Start Date"), DataType(DataType.DateTime)]
        public DateTime? ContractStartDate { get; set; } // Data di inizio contratto

        [AllowNull, Column(Order = 41), Display(Name = "Contract End Date"), DataType(DataType.DateTime)]
        public DateTime? ContractEndDate { get; set; } // Data di fine contratto

        [AllowNull, Column(TypeName = "decimal(18,2)", Order = 42), Display(Name = "Monthly Salary")]
        public decimal? MonthlySalary { get; set; } // Salario mensile

        [AllowNull, Column(TypeName = "decimal(18,2)", Order = 43), Display(Name = "Bonuses")]
        public decimal? Bonuses { get; set; } // Bonus annuali o straordinari

        [AllowNull, Column(TypeName = "decimal(18,2)", Order = 44), Display(Name = "Allowances")]
        public decimal? Allowances { get; set; } // Benefici extra (es. buoni pasto)
        #endregion

        #region Additional Attributes
        [AllowNull, Column(TypeName = "varchar(50)", Order = 50), Display(Name = "Employment Type")]
        public string? EmploymentType { get; set; } // Tipo di contratto (Es: Full-Time, Part-Time, Freelance)

        [AllowNull, Column(TypeName = "decimal(18,3)", Order = 51), Display(Name = "Overtime Rate")]
        public decimal? OvertimeRate { get; set; } // Tariffa straordinari

        [AllowNull, Column(TypeName = "text", Order = 52), Display(Name = "Skills")]
        /// <summary>
        /// Competenze formattate come JSON o CSV
        /// </summary>
        public string? Skills { get; set; } 

        [AllowNull, Column(Order = 53), Display(Name = "Supervisor ID"), ForeignKey("UserID")]
        /// <summary>
        /// ID del supervisore
        /// </summary>
        public Guid? SupervisorID { get; set; }
        #endregion

        #region Access Control
        [AllowNull, Column(TypeName = "varchar(50)", Order = 60), Display(Name = "Access Level")]
        public string? AccessLevel { get; set; } // Livello di accesso agli edifici o strumenti

        [AllowNull, Column(TypeName = "text", Order = 61), Display(Name = "Authorized Areas")]
        public string? AuthorizedAreas { get; set; } // Aree autorizzate (JSON o CSV)
        #endregion

        #region Additional Attributes
        [AllowNull, Column(TypeName = "text", Order = 70), Display(Name = "Internal Notes")]
        public string? InternalNotes { get; set; } // Note interne

        [AllowNull, Column(TypeName = "text", Order = 71), Display(Name = "Public Notes")]
        public string? PublicNotes { get; set; } // Note pubbliche visibili nei report

        [AllowNull, Column(TypeName = "text", Order = 72), Display(Name = "External System Reference")]
        public string? ExternalSystemReference { get; set; } // Collegamenti a sistemi esterni

        [AllowNull, Column(TypeName = "varchar(50)", Order = 73), Display(Name = "Sync Status")]
        public string? SyncStatus { get; set; } // Stato di sincronizzazione
        #endregion

        #region Deletititon
        [Required, NotNull, Column(Order = 801), Display(Name = "Is deleted ?"), DefaultValue("0")]
        public bool IsDeleted { get; set; } = false;

        [AllowNull, Column(Order = 802), Display(Name = "Who deleted this ?")]
        public Guid? IsDeletedBy { get; set; }

        [AllowNull, Column(TypeName = "text", Order = 803), Display(Name = "Why is deleted ?")]
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

        #region Navigation Properties

        [ForeignKey("UserID")]
        public virtual AspNetUser? User { get; set; }

        #endregion
    }
}
