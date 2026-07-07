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
    /// <summary>
    /// Can be null, it will help agensts to group property that are in a building
    /// </summary>
    [PrimaryKey(nameof(UserID), nameof(LoginProvider))]
    [Table(name: "AspNetUsersTokens")]
    public class AspNetUserToken
    {
        [Required, NotNull, Key, Column(Order = 0), DefaultValue("gen_random_uuid()")]
        public Guid ID { get; set; } = Guid.NewGuid();

        [Required, NotNull, ForeignKey("UserID"), Column(Order = 1)]
        public Guid UserID { get; set; }

        [Required, NotNull, Key, Column(TypeName = "varchar(128)", Order = 2)]
        public required string LoginProvider { get; set; }

        [Required, NotNull, Column(TypeName = "varchar(128)", Order = 3)]
        public required string Name { get; set; }

        [Required, NotNull, Column(TypeName = "text", Order = 4)]
        public required string Value { get; set; }

        #region Logging
        [Required, Display(Name = "Creation Date"), Column(TypeName = "timestamp", Order = 10)]
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;

        [AllowNull, Display(Name = "Expiration Date"), Column(TypeName = "timestamp", Order = 11)]
        public DateTime? ExpirationDate { get; set; }

        [AllowNull, Display(Name = "Created By"), Column(TypeName = "varchar(128)", Order = 12)]
        public Guid? CreatedBy { get; set; }
        #endregion

        #region Token State
        [Required, Display(Name = "Is Active"), Column(TypeName = "boolean", Order = 20)]
        public bool IsActive { get; set; } = true;

        [AllowNull, Display(Name = "Revoked On"), Column(TypeName = "timestamp", Order = 21)]
        public DateTime? RevokedOn { get; set; }

        [AllowNull, Display(Name = "Revocation Reason"), Column(TypeName = "varchar(256)", Order = 22)]
        public string? RevocationReason { get; set; }
        #endregion

        #region Token Usage
        [AllowNull, Display(Name = "IP Address"), Column(TypeName = "varchar(45)", Order = 30)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// Informazioni sul dispositivo/browser che ha utilizzato il token.
        /// </summary>
        [AllowNull, Display(Name = "User Agent"), Column(TypeName = "varchar(512)", Order = 31)]
        public string? UserAgent { get; set; }
        #endregion

        #region Token Security
        [Required, Display(Name = "Attempt Count"), Column(TypeName = "int", Order = 40)]
        public int AttemptCount { get; set; } = 0;

        /// <summary>
        /// TODO: Prevedere LookUp
        /// Per classificare i token (es. "EmailConfirmation", "PasswordReset")
        /// </summary>
        [AllowNull, Display(Name = "Token Type"), Column(TypeName = "varchar(128)", Order = 41)]
        public string? TokenType { get; set; }
        #endregion

        /// <summary>
        /// Un campo JSON o XML per archiviare dati personalizzati associati al token
        /// </summary>
        [AllowNull, Display(Name = "Metadata"), Column(TypeName = "text", Order = 50)]
        public string? Metadata { get; set; }
    }
}
