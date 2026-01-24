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

namespace Shared.Models.DTOs
{
    public class Payment_DocumentType_Detail_DTO
    {
        public Guid DocumentTypeID { get; set; }

        #region Document Details
        [Required, Display(Name = "DocumentTypeName")]
        public required string DocumentTypeName { get; set; } = string.Empty; // e.g., "Invoice", "Receipt"

        [Required]
        /// <summary>
        /// Document type description.
        /// </summary>
        public string? DocumentTypeDescription { get; set; }

        [AllowNull]
        /// <summary>
        /// Icon for the document type.
        /// </summary>
        public string? DocumentTypeIcon { get; set; }
        #endregion



        [Required]
        /// <summary>
        /// Indicates whether the document type is visible.
        /// </summary>
        public bool Visible { get; set; } = true;

        [AllowNull]
        /// <summary>
        /// Display order for the document type.
        /// </summary>
        public int DisplayOrder { get; set; } = 999;

        #region For custom values
        [AllowNull]
        public Guid? TenantID { get; set; }
        #endregion

        #region Deletititon
        [Required, NotNull]
        public bool IsDeleted { get; set; } = false;

        [AllowNull, Display(Name = "Who deleted this Customer ?")]
        public Guid? IsDeletedBy { get; set; }

        [AllowNull, Display(Name = "Why is Customer deleted ?")]
        public string? IsDeletedWhy { get; set; }

        [AllowNull, Display(Name = "Date deleted"), DataType(DataType.DateTime)]
        public DateTime? DateDeleted { get; set; }
        #endregion

        #region Audit & Tracking
        [AllowNull, Display(Name = "Created By")]
        public Guid? CreatedBy { get; set; }

        [Required, NotNull, Display(Name = "Created"), DataType(DataType.DateTime)]
        public DateTime DateIns { get; set; } = DateTime.UtcNow;

        [AllowNull, Display(Name = "Edited By")]
        public Guid? EditedBy { get; set; }

        [AllowNull, Display(Name = "Date edit"), DataType(DataType.DateTime)]
        public DateTime? DateEdit { get; set; }
        #endregion

    }
}
