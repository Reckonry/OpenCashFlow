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

namespace Shared.Models
{
    [Table(name: "Companies_LookUp_Statuses")]
    [PrimaryKey(nameof(StatusID))]
    public class Company_LookUp_Status
    {
        [Required, NotNull, Key, Column(Order = 0), DefaultValue("gen_random_uuid()")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid StatusID { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Provide a status name"), NotNull, Column(TypeName = "varchar(256)", Order = 1), Display(Name = "Status Name")]
        public required string StatusName { get; set; }

        [AllowNull, Column(TypeName = "text", Order = 2), Display(Name = "Status Description")]
        public string? StatusDescription { get; set; }

        [AllowNull, Column(TypeName = "varchar(100)", Order = 3), Display(Name = "Status Color")]
        public string? StatusColor { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 4), Display(Name = "Status Icon")]
        public string? StatusIcon { get; set; }

        [Required, NotNull, Column(Order = 5), Display(Name = "Visible")]
        public bool Visible { get; set; } = true;

        [Required, NotNull, Column(Order = 6), Display(Name = "Position")]
        public int DisplayOrder { get; set; } = 999;

        #region For custom values
        [AllowNull, Column(Order = 700), ForeignKey("TenantID")]
        public Guid? TenantID { get; set; }
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
    }
}
