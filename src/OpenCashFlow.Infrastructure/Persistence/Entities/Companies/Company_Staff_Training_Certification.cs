using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace OpenCashFlow.Infrastructure.Persistence.Entities
{
    [Table(name: "Companies_Staff_Training_Certifications")]
    [PrimaryKey(nameof(TrainingID))]
    public class Company_Staff_Training_Certification
    {
        #region Data Linking
        [Required, NotNull, Column(Order = 1), ForeignKey("TenantID")]
        public Guid TenantID { get; set; } // Collegamento all'azienda

        [Required, NotNull, Column(Order = 2), ForeignKey("UserID")]
        public Guid UserID { get; set; } // Collegamento al dipendente
        #endregion

        #region Identification
        [Required, Key, Column(Order = 0), DefaultValue("NewID()")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid TrainingID { get; set; } = Guid.NewGuid();
        #endregion

        #region Training/Certification Details
        [Required(ErrorMessage = "Provide a name"), NotNull, Column(TypeName = "text", Order = 10), Display(Name = "Title")]
        public required string Title { get; set; } // Nome della formazione o certificazione

        [AllowNull, Column(TypeName = "text", Order = 11), Display(Name = "Description")]
        public string? Description { get; set; } // Descrizione

        [AllowNull, Column(Order = 12), Display(Name = "Training Start Date"), DataType(DataType.DateTime)]
        public DateTime? StartDate { get; set; } // Data di inizio

        [AllowNull, Column(Order = 13), Display(Name = "Training End Date"), DataType(DataType.DateTime)]
        public DateTime? EndDate { get; set; } // Data di fine

        [AllowNull, Column(TypeName = "varchar(50)", Order = 14), Display(Name = "Type")]
        public string? Type { get; set; } // e.g., Mandatory training, Certification, Workshop

        [AllowNull, Column(Order = 15), Display(Name = "Is Mandatory"), DefaultValue(false)]
        public bool IsMandatory { get; set; } = false; // Mandatory training?

        [AllowNull, Column(TypeName = "varchar(256)", Order = 16), Display(Name = "Provider")]
        public string? Provider { get; set; } // Name of the issuing organization or provider

        [AllowNull, Column(Order = 17), Display(Name = "Training Location")]
        public string? Location { get; set; } // Training location

        [AllowNull, Column(TypeName = "varchar(50)", Order = 18), Display(Name = "Status")]
        public string? Status { get; set; } = "Planned"; // e.g., Planned, In Progress, Completed, Expired

        [AllowNull, Column(TypeName = "varchar(50)", Order = 19), Display(Name = "Certification Level")]
        public string? CertificationLevel { get; set; } // Certification level (e.g., Basic, Advanced)
        #endregion

        #region Validity and Expiration
        [AllowNull, Column(Order = 20), Display(Name = "Validity Start Date"), DataType(DataType.DateTime)]
        public DateTime? ValidFrom { get; set; } // Validity start date

        [AllowNull, Column(Order = 21), Display(Name = "Validity End Date"), DataType(DataType.DateTime)]
        public DateTime? ValidTo { get; set; } // Validity end date

        [AllowNull, Column(TypeName = "text", Order = 22), Display(Name = "Renewal Requirements")]
        public string? RenewalRequirements { get; set; } // Renewal requirements
        #endregion

        #region Results and Feedback
        [AllowNull, Column(TypeName = "decimal(18,2)", Order = 30), Display(Name = "Score")]
        public decimal? Score { get; set; } // Score achieved (if applicable)

        [AllowNull, Column(TypeName = "text", Order = 31), Display(Name = "Feedback")]
        public string? Feedback { get; set; } // Feedback received or provided

        [AllowNull, Column(TypeName = "text", Order = 32), Display(Name = "Skills Acquired")]
        public string? SkillsAcquired { get; set; } // Skills acquired (formatted as JSON or CSV)
        #endregion

        #region Attachments
        [AllowNull, Column(TypeName = "varchar(256)", Order = 40), Display(Name = "Certificate Path")]
        public string? CertificatePath { get; set; } // Path to the certification file

        [AllowNull, Column(TypeName = "varchar(256)", Order = 41), Display(Name = "Training Material Path")]
        public string? TrainingMaterialPath { get; set; } // Training materials
        #endregion

        #region Approval and Audit
        [AllowNull, Column(Order = 50), Display(Name = "Approved By"), ForeignKey("UserID")]
        public Guid? ApprovedBy { get; set; } // User who approved

        [AllowNull, Column(Order = 51), Display(Name = "Approval Date"), DataType(DataType.DateTime)]
        public DateTime? ApprovalDate { get; set; } // Approval date
        #endregion

        #region Notes and Tags
        [AllowNull, Column(TypeName = "text", Order = 60), Display(Name = "Notes")]
        public string? Notes { get; set; } // General notes

        [AllowNull, Column(TypeName = "text", Order = 61), Display(Name = "Tags")]
        public string? Tags { get; set; } // Tags for categorization
        #endregion

        #region Deletion
        [Required, NotNull, Column(Order = 801), Display(Name = "Is Deleted?"), DefaultValue(false)]
        public bool IsDeleted { get; set; } = false;

        [AllowNull, Column(Order = 802), Display(Name = "Who Deleted This?"), ForeignKey("UserID")]
        public Guid? IsDeletedBy { get; set; } // User who deleted

        [AllowNull, Column(TypeName = "text", Order = 803), Display(Name = "Why Is Deleted?")]
        public string? IsDeletedWhy { get; set; } // Reason it was deleted

        [AllowNull, Column(Order = 805), Display(Name = "Date deleted"), DataType(DataType.DateTime)]
        public DateTime? DateDeleted { get; set; }
        #endregion

        #region Audit & Tracking
        [AllowNull, Column(Order = 900), Display(Name = "Created By"), ForeignKey("UserID")]
        public Guid? CreatedBy { get; set; } // User who created the record

        [Required, NotNull, Column(Order = 901), DefaultValue("now()")]
        [Display(Name = "Created"), DataType(DataType.DateTime)]
        public DateTime DateIns { get; set; } = DateTime.UtcNow; // Creation date

        [AllowNull, Column(Order = 902), Display(Name = "Edited By"), ForeignKey("UserID")]
        public Guid? EditedBy { get; set; } // User who edited the record

        [AllowNull, Column(Order = 903), Display(Name = "Date Edit"), DataType(DataType.DateTime)]
        public DateTime? DateEdit { get; set; } // Edit date
        #endregion
    }
}
