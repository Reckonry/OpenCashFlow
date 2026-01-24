using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Shared.Models.Identity
{
    [Table(name: "AspNetUserClaims")]
    [PrimaryKey(nameof(ClaimID))]
    public class AspNetUserClaim
    {
        [Key, Required, Column(Order = 0), DefaultValue("gen_random_uuid()")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ClaimID { get; set; } = Guid.NewGuid();

        [Column(Order = 1), Required, NotNull, ForeignKey("UserID")]
        public Guid UserID { get; set; }

        [Column(TypeName = "text", Order = 2)]
        public required string ClaimType { get; set; }

        [Column(TypeName = "text", Order = 3)]
        public string? ClaimValue { get; set; } = true.ToString();

        #region CustomProp
        [Required, NotNull, Display(Name = "Is Role visible ?"), Column(Order = 5), DefaultValue(true)]
        public bool IsVisible { get; set; } = true;
        #endregion

        #region Deletititon
        [Required, NotNull, Column(Order = 802), Display(Name = "Is Customer deleted ?"), DefaultValue("0")]
        public bool IsDeleted { get; set; } = false;

        [AllowNull, Column(Order = 803), Display(Name = "Who deleted this Customer ?")]
        public Guid? IsDeletedBy { get; set; }

        [AllowNull, Column(TypeName = "text", Order = 804), Display(Name = "Why is Customer deleted ?")]
        public string? IsDeletedWhy { get; set; }
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
