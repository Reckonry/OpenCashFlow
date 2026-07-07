using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace OpenCashFlow.Infrastructure.Persistence.Entities
{
    [Table(name: "Companies_BillingAddresses")]
    [PrimaryKey(nameof(BillingAddressID))]
    public class Company_BillingAddress
    {
        #region Data Linking
        [AllowNull, Column(Order = 0), ForeignKey("TenantID")]
        public Guid? TenantID { get; set; }
        #endregion

        [Required, NotNull, Key, Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid BillingAddressID { get; set; } = Guid.NewGuid();

        [AllowNull, Column(TypeName = "varchar(256)", Order = 28), Display(Name = "Address")]
        public string? BillingAddress { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 29), Display(Name = "Address Num")]
        public string? BillingAddressNum { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 30), Display(Name = "Int.")]
        public string? BillingAddressInt { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 31), Display(Name = "Stair")]
        public string? BillingAddressStair { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 32), Display(Name = "ZIP")]
        public string? BillingZIP { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 33), Display(Name = "City")]
        public string? BillingCity { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 34), Display(Name = "State")]
        public string? BillingState { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 35), Display(Name = "Country")]
        public string? BillingCountry { get; set; }

        [NotMapped]
        public string? BillingFullAddress
        {
            get =>
              (string.IsNullOrEmpty(BillingAddress) ? null : BillingAddress + (string.IsNullOrEmpty(BillingAddressNum) ? null : " n." + BillingAddressNum) + ", ") +
              (string.IsNullOrEmpty(BillingZIP) ? null : BillingZIP + " - ") +
              (string.IsNullOrEmpty(BillingCity) ? null : BillingCity + " ") + (string.IsNullOrEmpty(BillingState) ? null : "(" + BillingState + ") ") + BillingCountry?.Trim();
        }

        [AllowNull, Column(TypeName = "boolean", Order = 36), Display(Name = "Is Primary")]
        public bool IsPrimary { get; set; } = false;

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
