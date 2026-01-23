using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Core
{
    [Table(name: "Core_Contacts_Email_LookUp_Types")]
    public class Core_Contacts_Email_LookUp_Type
    {
        [Required, NotNull, Key, Column(Order = 0), DefaultValue("gen_random_uuid()")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        /// <summary>
        /// Identificativo univoco del tipo di documento.
        /// </summary>
        public Guid ContactEmailTypeID { get; set; } = Guid.NewGuid();

        #region Type Details
        [Required, Column(TypeName = "varchar(256)", Order = 1)]
        /// <summary>
        /// Nome del tipo di telefono.
        /// </summary>
        public required string ContactTypeName { get; set; }

        [Required, Column(TypeName = "varchar(50)", Order = 2)]
        /// <summary>
        /// Codice univoco del tipo di telefono.
        /// </summary>
        public required string ContactTypeCode { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 3)]
        /// <summary>
        /// Descrizione del tipo di tel .
        /// </summary>
        public string? ContactTypeDescription { get; set; }

        [AllowNull, Column(TypeName = "text", Order = 4)]
        /// <summary>
        /// Icona per il tipo di telefoni.
        /// </summary>
        public string? ContactTypeIcon { get; set; }
        #endregion

        [AllowNull, Column(TypeName = "text", Order = 5)]
        /// <summary>
        /// Validation rules associated with the document type (e.g., file name format).
        /// </summary>
        public string? ValidationRules { get; set; }

        [AllowNull, Column(TypeName = "text", Order = 6)]
        /// <summary>
        /// Tags associated with the document type (e.g., "Legal", "Contractual").
        /// </summary>
        public string? Tags { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 7)]
        /// <summary>
        /// External reference for integrations with other systems.
        /// </summary>
        public string? ExternalReferenceID { get; set; }


        [Required, Column(Order = 8), DefaultValue(true)]
        /// <summary>
        /// Indicates whether the document type is visible.
        /// </summary>
        public bool Visible { get; set; } = true;

        [AllowNull, Column(Order = 9)]
        /// <summary>
        /// Display order for the document type.
        /// </summary>
        public int? DisplayOrder { get; set; }

        #region For custom values
        [AllowNull, Column(Order = 701), ForeignKey("GMRCompanyID")]
        public Guid? GMRCompanyID { get; set; }
        #endregion

        #region Deletititon
        [Required, NotNull, Column(Order = 800), Display(Name = "Is deleted ?"), DefaultValue("0")]
        public bool IsDeleted { get; set; } = false;

        [AllowNull, Column(Order = 801), Display(Name = "Who deleted this ?")]
        public Guid? IsDeletedBy { get; set; }

        [AllowNull, Column(TypeName = "text", Order = 802), Display(Name = "Why is deleted ?")]
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

        #endregion
    }
}
