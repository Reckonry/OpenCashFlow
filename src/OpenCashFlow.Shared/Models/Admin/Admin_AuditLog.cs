using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Shared.Models.Admin
{
    [Table("Admin_AuditLog")]
    [PrimaryKey(nameof(AuditLogID))]
    [Index(nameof(EventType), nameof(Timestamp))]
    [Index(nameof(UserID), nameof(Timestamp))]
    [Index(nameof(Resource), nameof(ResourceID))]
    public class Admin_AuditLog
    {
        [Key, Column(Order = 0), DefaultValue("gen_random_uuid()")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid AuditLogID { get; set; } = Guid.NewGuid();

        [Required, NotNull, Column(TypeName = "varchar(100)", Order = 1)]
        [Display(Name = "Event Type")]
        public required string EventType { get; set; }

        [Required, NotNull, Column(TypeName = "varchar(100)", Order = 2)]
        [Display(Name = "Resource")]
        public required string Resource { get; set; }

        [AllowNull, Column(TypeName = "varchar(100)", Order = 3)]
        [Display(Name = "Resource ID")]
        public string? ResourceID { get; set; }

        [Required, NotNull, Column(TypeName = "varchar(100)", Order = 4)]
        [Display(Name = "Action")]
        public required string Action { get; set; }

        [AllowNull, Column(Order = 5)]
        [Display(Name = "User ID")]
        public Guid? UserID { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 6)]
        [Display(Name = "Username")]
        public string? Username { get; set; }

        [AllowNull, Column(TypeName = "text", Order = 7)]
        [Display(Name = "Changes (JSON)")]
        public string? Changes { get; set; }

        [AllowNull, Column(TypeName = "varchar(50)", Order = 8)]
        [Display(Name = "IP Address")]
        public string? IPAddress { get; set; }

        [AllowNull, Column(TypeName = "text", Order = 9)]
        [Display(Name = "User Agent")]
        public string? UserAgent { get; set; }

        [Required, NotNull, Column(Order = 10), DefaultValue("now()")]
        [Display(Name = "Timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [AllowNull, Column(TypeName = "varchar(50)", Order = 11)]
        [Display(Name = "Severity")]
        public string? Severity { get; set; } // Info, Warning, Error, Critical

        [AllowNull, Column(TypeName = "text", Order = 12)]
        [Display(Name = "Additional Info")]
        public string? AdditionalInfo { get; set; }

        [AllowNull, Column(Order = 13)]
        [Display(Name = "Company ID")]
        public Guid? TenantID { get; set; }
    }
}
