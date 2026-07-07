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

namespace OpenCashFlow.Infrastructure.Persistence.Entities
{
    [Table(name: "Companies_Addresses")]
    [PrimaryKey(nameof(CompanyAddressID))]
    public class Company_Address
    {
        #region Data Linking
        [AllowNull, Column(Order = 0), ForeignKey("TenantID")]
        public Guid? TenantID { get; set; }
        #endregion

        [Required, NotNull, Key, Column(Order = 5), DefaultValue("gen_random_uuid()")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid CompanyAddressID { get; set; } = Guid.NewGuid();

        #region Address fields
        [Required, Column(TypeName = "varchar(256)", Order = 10), Display(Name = "Address")]
        public string? Address { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 11), Display(Name = "Num.")]
        public string? AddressNum { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 12), Display(Name = "Int")]
        public string? AddressInt { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 13), Display(Name = "Stair")]
        public string? AddressStair { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 14), Display(Name = "City")]
        public string? City { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 15), Display(Name = "ZIP")]
        public string? ZIP { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 16), Display(Name = "State")]
        public string? State { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 17), Display(Name = "Country")]
        public string? Country { get; set; }

        [NotMapped]
        public string? FullAddress
        {
            get =>
              (string.IsNullOrEmpty(Address) ? null : Address + (string.IsNullOrEmpty(AddressNum) ? null : " n." + AddressNum) + ", ") +
              (string.IsNullOrEmpty(City) ? null : City + " ") + (string.IsNullOrEmpty(State) ? null : "(" + State + ") ") + Country?.Trim();
        }

        [AllowNull, Column(TypeName = "text", Order = 18), Display(Name = "Address Note")]
        public string? AddressNote { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 19), Display(Name = "Geo Hash")]
        public string? GeoHash { get; set; }
        #endregion

        [AllowNull, Column(Order = 20), Display(Name = "Status"), ForeignKey("AddressTypeID")]
        public Guid? AddressTypeID { get; set; }

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
