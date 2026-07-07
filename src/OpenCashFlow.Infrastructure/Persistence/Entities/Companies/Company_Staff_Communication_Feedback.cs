using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace OpenCashFlow.Infrastructure.Persistence.Entities
{
    [Table(name: "Companies_Staff_Communication_Feedback")]
    [PrimaryKey(nameof(FeedbackID))]
    public class Company_Staff_Communication_Feedback
    {
        #region Data Linking
        [Required, NotNull, Column(Order = 0), ForeignKey("TenantID")]
        public Guid TenantID { get; set; } // Collegamento all'azienda

        [AllowNull, Column(Order = 1), ForeignKey("UserID")]
        public Guid? UserID { get; set; } // Collegamento al dipendente che invia/riceve feedback
        #endregion

        #region Feedback Identification
        [Required, Key, Column(Order = 10), DefaultValue("NewID()")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid FeedbackID { get; set; } = Guid.NewGuid();
        #endregion

        #region Feedback Details
        [Required(ErrorMessage = "Provide a feedback type"), NotNull, Column(TypeName = "varchar(50)", Order = 20), Display(Name = "Feedback Type")]
        public string FeedbackType { get; set; } = "General"; // Es: Generale, Problema, Suggerimento

        [Required(ErrorMessage = "Provide a title"), NotNull, Column(TypeName = "varchar(256)", Order = 21), Display(Name = "Title")]
        public required string Title { get; set; } // Titolo del feedback

        [AllowNull, Column(TypeName = "text", Order = 22), Display(Name = "Message")]
        public string? Message { get; set; } // Contenuto del feedback

        [AllowNull, Column(TypeName = "varchar(256)", Order = 23), Display(Name = "Attachment Path")]
        public string? AttachmentPath { get; set; } // Allegati (se applicabile)
        #endregion

        #region Feedback Status
        [Required, NotNull, Column(Order = 30), Display(Name = "Is Resolved?"), DefaultValue(false)]
        public bool IsResolved { get; set; } = false; // Stato del feedback

        [AllowNull, Column(TypeName = "varchar(256)", Order = 31), Display(Name = "Resolution Notes")]
        public string? ResolutionNotes { get; set; } // Note di risoluzione

        [AllowNull, Column(Order = 32), Display(Name = "Resolved By"), ForeignKey("UserID")]
        public Guid? ResolvedBy { get; set; } // ID della persona che ha risolto il feedback

        [AllowNull, Column(Order = 33), Display(Name = "Resolution Date"), DataType(DataType.DateTime)]
        public DateTime? ResolutionDate { get; set; } // Data di risoluzione
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
