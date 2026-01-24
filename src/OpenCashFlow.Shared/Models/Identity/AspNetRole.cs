using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Shared.Models.Identity
{
    [Table(name: "AspNetRoles")]
    [PrimaryKey(nameof(RoleID))]
    public class AspNetRole
    {
        [Required, Key, Column(Order = 0), DefaultValue("gen_random_uuid()")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid RoleID { get; set; } = Guid.NewGuid();

        [Required, Display(Name = "Role Name"), NotNull, StringLength(256), Column(Order = 1)]
        public required string RoleName { get; set; }

        [StringLength(256), Display(Name = "Normalized Role name"), Column(Order = 2)]
        public string? NormalizedName { get => Regex.Replace(RoleName, "[^0-9a-zA-Z@._-]+", "_").ToLower(); }

        [Column(Order = 3)]
        public string? ConcurrencyStamp { get; set; }

        // <summary> Permessi concessi al ruolo. </summary>
        [JsonIgnore]
        public virtual ICollection<AspNetRolePermission> RolePermissions { get; set; } = new List<AspNetRolePermission>();

        [JsonIgnore]
        public virtual ICollection<AspNetUserRole> UserRoles { get; set; } = new List<AspNetUserRole>();

        #region CustomProp
        [AllowNull, Column(Order = 10), Display(Name = "Role Image"), StringLength(256)]
        public string? RoleImage { get; set; }

        [Required, NotNull, Display(Name = "Is Role visible ?"), Column(Order = 11), DefaultValue(true)]
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
