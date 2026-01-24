using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;
using System.ComponentModel;

namespace Shared.Models.Companies
{
    [Table(name: "Companies_APIKeys")]
    [PrimaryKey(nameof(ID))]
    public class Company_API_Key
    {
        [Required, NotNull, Key, Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ID { get; set; } = Guid.NewGuid();

        [Required, NotNull, Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid TenantID { get; set; }

        [Required(ErrorMessage = "Provide a key type"), NotNull, Column(TypeName = "varchar(256)", Order = 2), Display(Name = "API Key Type")]
        public required CoreAPIKey APIKeyType { get; set; }

        [Required(ErrorMessage = "Provide a key name"), Column(TypeName = "varchar(254)", Order = 3), Display(Name = "Key Name")]
        public required string KeyName { get; set; }

        [Required, Column(TypeName = "uuid", Order = 4), Display(Name = "Key Value")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid KeyValue { get; set; } = Guid.NewGuid();

        #region Deletititon
        [Required, NotNull, Column(Order = 802), Display(Name = "Is deleted ?"), DefaultValue("0")]
        public bool IsDeleted { get; set; } = false;

        [AllowNull, Column(Order = 803), Display(Name = "Who deleted this ?")]
        public Guid? IsDeletedBy { get; set; }

        [AllowNull, Column(TypeName = "text", Order = 804), Display(Name = "Why is deleted ?")]
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