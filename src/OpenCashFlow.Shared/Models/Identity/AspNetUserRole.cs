using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Identity
{
    [Table(name: "AspNetUsersRoles")]
    [PrimaryKey(nameof(UserID), nameof(RoleID))]
    public class AspNetUserRole
    {
        [Required, NotNull, Key]
        public required Guid UserID { get; set; }

        /// <summary>
        /// Navigazione a dettagli User
        /// </summary>
        [ForeignKey(nameof(UserID))]
        public virtual AspNetUser? AspNetUser { get; set; }

        [Required, NotNull, Key]
        public required Guid RoleID { get; set; }

        /// <summary>
        /// Navigazione a dettagli Rolee
        /// </summary>
        [ForeignKey(nameof(RoleID))]
        public virtual AspNetRole? AspNetRole { get; set; }
    }
}
