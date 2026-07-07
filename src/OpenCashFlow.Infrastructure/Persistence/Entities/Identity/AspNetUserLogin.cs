using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace OpenCashFlow.Infrastructure.Persistence.Entities.Identity
{
    [Table(name: "AspNetUsersLogins")]
    [PrimaryKey(nameof(LoginProvider), nameof(ProviderKey))]
    public class AspNetUserLogin
    {
        [Required, Key, Column(TypeName = "varchar(128)", Order = 0)]
        public required string LoginProvider { get; set; }

        [Required, Key, Column(TypeName = "varchar(128)", Order = 1)]
        public required string ProviderKey { get; set; }

        [Column(TypeName = "text", Order = 2)]
        public string? ProviderDisplayName { get; set; }

        [Column(Order = 3), ForeignKey("UserID")]
        public required string UserID { get; set; }

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
