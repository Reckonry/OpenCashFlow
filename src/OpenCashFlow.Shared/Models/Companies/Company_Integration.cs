using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Shared.Models
{
    [Table(name: "Companies_Integrations")]
    [PrimaryKey(nameof(IntegrationID))]
    public class Company_Integration
    {
        #region Data Linking
        [Required, NotNull, Column(Order = 1), ForeignKey("TenantID")]
        public Guid TenantID { get; set; } // Collegamento all'azienda
        #endregion

        #region Integration Details
        [Required, Key, Column(Order = 0), DefaultValue("NewID()")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid IntegrationID { get; set; } = Guid.NewGuid(); // ID dell'integrazione

        [Required, NotNull, Column(TypeName = "varchar(256)", Order = 10), Display(Name = "Integration Name")]
        public required string IntegrationName { get; set; } // Nome dell'integrazione (es. "Workday", "SAP")

        [AllowNull, Column(TypeName = "text", Order = 11), Display(Name = "Description")]
        public string? Description { get; set; } // Descrizione dell'integrazione

        [Required, NotNull, Column(TypeName = "varchar(256)", Order = 12), Display(Name = "API Endpoint")]
        public required string ApiEndpoint { get; set; } // Endpoint API del sistema HR esterno

        [AllowNull, Column(TypeName = "varchar(256)", Order = 13), Display(Name = "Integration Key")]
        public string? IntegrationKey { get; set; } // Chiave di integrazione o identificatore

        [AllowNull, Column(TypeName = "varchar(256)", Order = 14), Display(Name = "Authentication Type")]
        public string? AuthenticationType { get; set; } // Tipo di autenticazione (es. OAuth2, API Key)
        #endregion

        #region Monitoring and Sync Details
        [AllowNull, Column(TypeName = "varchar(50)", Order = 20), Display(Name = "Sync Frequency")]
        public string? SyncFrequency { get; set; } // Sync frequency (e.g., daily, weekly)

        [AllowNull, Column(TypeName = "varchar(50)", Order = 21), Display(Name = "Last Sync Status")]
        public string? LastSyncStatus { get; set; } // Status of the last sync (e.g., Success, Failed)

        [AllowNull, Column(Order = 22), Display(Name = "Last Sync Date"), DataType(DataType.DateTime)]
        public DateTime? LastSyncDate { get; set; } // Last sync date

        [AllowNull, Column(TypeName = "text", Order = 23), Display(Name = "Error Logs")]
        public string? ErrorLogs { get; set; } // Sync error logs
        #endregion

        #region Configuration and Status
        [AllowNull, Column(TypeName = "varchar(256)", Order = 30), Display(Name = "Configuration Settings")]
        public string? ConfigurationSettings { get; set; } // Specific configuration settings (e.g., JSON)

        [Required, NotNull, Column(Order = 31), Display(Name = "Is Active"), DefaultValue(true)]
        public bool IsActive { get; set; } = true; // Indicates whether the integration is active

        [AllowNull, Column(Order = 32), Display(Name = "External Reference")]
        public string? ExternalReference { get; set; } // External reference to the integrated system
        #endregion

        #region Deletion
        [Required, NotNull, Column(Order = 801), Display(Name = "Is deleted?"), DefaultValue(false)]
        public bool IsDeleted { get; set; } = false; // Indicates whether the integration is deleted

        [AllowNull, Column(Order = 802), Display(Name = "Who deleted this?"), ForeignKey("UserID")]
        public Guid? IsDeletedBy { get; set; } // User who deleted

        [AllowNull, Column(TypeName = "text", Order = 803), Display(Name = "Why is deleted?")]
        public string? IsDeletedWhy { get; set; } // Reason it was deleted

        [AllowNull, Column(Order = 805), Display(Name = "Date deleted"), DataType(DataType.DateTime)]
        public DateTime? DateDeleted { get; set; }
        #endregion

        #region Audit & Tracking
        [AllowNull, Column(Order = 900), Display(Name = "Created By"), ForeignKey("UserID")]
        public Guid? CreatedBy { get; set; } // User who created the integration

        [Required, NotNull, Column(Order = 901), DefaultValue("now()")]
        [Display(Name = "Created"), DataType(DataType.DateTime)]
        public DateTime DateIns { get; set; } = DateTime.UtcNow; // Creation date

        [AllowNull, Column(Order = 902), Display(Name = "Edited By"), ForeignKey("UserID")]
        public Guid? EditedBy { get; set; } // User who edited the integration

        [AllowNull, Column(Order = 903), Display(Name = "Date Edit"), DataType(DataType.DateTime)]
        public DateTime? DateEdit { get; set; } // Edit date
        #endregion
    }
}
