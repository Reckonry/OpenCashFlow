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
    [Table(name: "Companies_Contacts_Emails")]
    [PrimaryKey(nameof(ContactEmailID))]
    public class Company_Contact_Email
    {
        #region Data Linking
        [AllowNull, Column(Order = 0), ForeignKey("TenantID")]
        public Guid? TenantID { get; set; }
        #endregion

        [Required, NotNull, Key, Column(Order = 13), DefaultValue("gen_random_uuid()")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ContactEmailID { get; set; } = Guid.NewGuid();

        #region Contact details
        [AllowNull, Column(TypeName = "varchar(256)", Order = 11), Display(Name = "E-Mail")]
        public string? Email { get; set; }
        #endregion


        //todo: creare email type table
        [AllowNull, Column(Order = 50), Display(Name = "Status")]
        public Guid? EmailTypeID { get; set; }

        [AllowNull, Column(TypeName = "boolean", Order = 51), Display(Name = "E-mail Confirmed")]
        public bool EmailConfirmed { get; set; } = false;

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
