using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace OpenCashFlow.Infrastructure.Persistence.Entities
{
    [Table("Payments_DocumentTypes_LookUps")]
    [PrimaryKey(nameof(DocumentTypeID))]
    public class Payment_DocumentType_LookUp
    {
        [Required, NotNull, Key, Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid DocumentTypeID { get; set; } = Guid.NewGuid();

        #region Document Details
        [Required, Column(TypeName = "varchar(256)", Order = 10), Display(Name = "DocumentTypeName")]
        public required string DocumentTypeName { get; set; } = string.Empty; // e.g., "Invoice", "Receipt"

        [Required, Column(TypeName = "varchar(50)", Order = 11)]
        /// <summary>
        /// Descrizione del tipo di documento.
        /// </summary>
        public string? DocumentTypeDescription { get; set; }

        [AllowNull, Column(TypeName = "text", Order = 12)]
        /// <summary>
        /// Icon for the document type.
        /// </summary>
        public string? DocumentTypeIcon { get; set; }
        #endregion



        [Required, Column(Order = 13), DefaultValue(true)]
        /// <summary>
        /// Indicates whether the document type is visible.
        /// </summary>
        public bool Visible { get; set; } = true;

        [AllowNull, Column(Order = 14)]
        /// <summary>
        /// Display order for the document type.
        /// </summary>
        public int DisplayOrder { get; set; } = 999;

        #region For custom values
        [AllowNull, Column(Order = 701), ForeignKey("TenantID")]
        public Guid? TenantID { get; set; }
        #endregion

        #region Deletititon
        [Required, NotNull, Column(Order = 800), Display(Name = "Is Customer deleted ?"), DefaultValue(false)]
        public bool IsDeleted { get; set; } = false;

        [AllowNull, Column(Order = 801), Display(Name = "Who deleted this Customer ?"), ForeignKey("UserID")]
        public Guid? IsDeletedBy { get; set; }

        [AllowNull, Column(TypeName = "text", Order = 802), Display(Name = "Why is Customer deleted ?")]
        public string? IsDeletedWhy { get; set; }

        [AllowNull, Column(Order = 803), Display(Name = "Date deleted"), DataType(DataType.DateTime)]
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

        [AllowNull, Column(Order = 903), Display(Name = "Date edit"), DataType(DataType.DateTime)]
        public DateTime? DateEdit { get; set; }
        #endregion

    }
}
