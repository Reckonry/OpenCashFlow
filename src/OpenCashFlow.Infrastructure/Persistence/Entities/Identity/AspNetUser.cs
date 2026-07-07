using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpenCashFlow.Infrastructure.Persistence.Entities.Identity
{
    [Table(name: "AspNetUsers")]
    [PrimaryKey(nameof(UserID))]
    public class AspNetUser
    {
        [Required, Key, Column(Order = 0), DefaultValue("gen_random_uuid()")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid UserID { get; set; } = Guid.NewGuid();

        [Required, NotNull, Display(Name = "Username"), Column(TypeName = "varchar(256)", Order = 1)]
        [JsonPropertyName("username")]
        public required string UserName { get; set; }

        [Required, Display(Name = "Normalized Username"), Column(TypeName = "varchar(256)", Order = 2), JsonIgnore]
        public string? NormalizedUserName { get => Regex.Replace(UserName, "[^0-9a-zA-Z@_.-]+", "_").ToLower(); }

        #region User Customization Property
        [AllowNull, Display(Name = "User Avatar"), Column(Order = 10)]
        [JsonPropertyName("userAvatar")]
        public string? UserAvatar { get; set; }

        [Required, NotNull, Display(Name = "Language"), Column(Order = 11)]
        public string Language { get; set; } = "it";

        [Required, NotNull, Display(Name = "Country"), Column(Order = 12)]
        public string Country { get; set; } = "IT";

        [AllowNull, Display(Name = "Timezone"), Column(Order = 13)]
        public string? Timezone { get; set; }
        #endregion

        #region User privileges
        /// <summary> connected roles applied to the user. </summary>
        
        public virtual ICollection<AspNetUserRole> Roles { get; set; } = new List<AspNetUserRole>();

        /// <summary> Allowed privileges </summary>
        public virtual ICollection<AspNetUserPermission> AssignedPermissions { get; set; } = new List<AspNetUserPermission>();

        /// <summary> Denied privileges </summary>
        public virtual ICollection<AspNetUserDeniedPermission> DeniedPermissions { get; set; } = new List<AspNetUserDeniedPermission>();
        #endregion

        #region User Personal details
        #region Name - Surname
        [AllowNull, Display(Name = "User Title"), Column(Order = 20)]
        public string? UserTitle { get; set; }

        [Required(ErrorMessage = "Provide a first name"), NotNull, Display(Name = "First name"), Column(TypeName = "varchar(256)", Order = 21)]
        public required string UserFirstName { get; set; }

        [AllowNull, Display(Name = "Middlename"), Column(TypeName = "varchar(256)", Order = 22)]
        public string? UserMiddleName { get; set; }

        [AllowNull, Display(Name = "Surname"), Column(TypeName = "varchar(256)", Order = 23)]
        public string? UserLastName { get; set; }

        [NotMapped, Display(Name = "User Name Surname")]
        public string? EmployeeNameSurname { get => UserTitle + " " + UserFirstName + " " + UserLastName; }
        [NotMapped, Display(Name = "User Surname Name")]
        public string? EmployeeSurnameName { get => UserTitle + " " + UserLastName + " " + UserFirstName; }
        #endregion

        #region Email and phone
        [Required, NotNull, Column(TypeName = "varchar(256)", Order = 30), Display(Name = "E-Mail")]
        [EmailAddress(ErrorMessage = "E-mail address not valid")]
        public required string Email { get; set; }

        [Column(TypeName = "varchar(256)", Order = 31)]
        public string? NormalizedEmail { get => Regex.Replace(Email, "[^0-9a-zA-Z]+", "_"); }

        [Required, Column(Order = 32), Display(Name = "Is Email Confirmed ?")]
        public bool EmailConfirmed { get; set; } = false;

        [AllowNull, Column(TypeName = "varchar(128)", Order = 33), Display(Name = "Mobile Country Code Prefix")]
        public string? PhoneNumberPrefix { get; set; }

        [AllowNull, Column(TypeName = "text", Order = 34), Display(Name = "Mobile Number")]
        public string? PhoneNumber { get; set; }

        [Required, NotNull, Column(Order = 35), Display(Name = "Is mobile phone confirmed ?")]
        public bool PhoneNumberConfirmed { get; set; } = false;
        #endregion

        #region GenZ stuff
        /// <summary>
        /// Based on the enum GenderTypes
        /// </summary>
        [AllowNull, Display(Name = "Gender"), Column(Order = 50)]
        public string? Gender { get; set; }

        /// <summary>
        /// Based on the enum PronounsTypes
        /// </summary>
        [AllowNull, Display(Name = "Pronouns"), Column(Order = 51)]
        public string? Pronouns { get; set; }
        #endregion

        #region Birth & nationality
        [AllowNull, Display(Name = "Date of Birth"), Column(Order = 60)]
        public DateOnly? DoB { get; set; }

        [AllowNull, Display(Name = "City of Birth"), Column(Order = 61)]
        public string? PoB { get; set; }

        [AllowNull, Display(Name = "State of Birth"), Column(Order = 62)]
        public string? SoB { get; set; }

        [AllowNull, Display(Name = "Country of Birth"), Column(Order = 63)]
        public string? CoB { get; set; }

        [AllowNull, Display(Name = "Nationality"), Column(Order = 64)]
        public string? Nationality { get; set; }
        #endregion
        #endregion

        #region User Accepted Terms & Conditions
        [Required, Column(Order = 70), Display(Name = "Accepts Privacy Policy"), DefaultValue(false)]
        public bool PrivacyPolicyAcepted { get; set; } = false;

        [AllowNull, Column(Order = 71), Display(Name = "Accepts Privacy Policy")]
        public string? PrivacyPolicyVersion { get; set; }

        [AllowNull, Column(Order = 72), Display(Name = "Privacy Policy Accepted Date")]
        public DateTime? PrivacyPolicyAcceptedDate { get; set; }
        #endregion

        #region Password & Login
        [Column(TypeName = "text", Order = 80), JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;

        [Column(TypeName = "text", Order = 81), JsonIgnore]
        public string PasswordSalt { get; set; } = string.Empty;

        [Column(TypeName = "text", Order = 82), JsonIgnore]
        public string? QuickLoginPinHash { get; set; }

        [AllowNull, Display(Name = "Quick Login Pin Valid Until"), Column(Order = 83)]
        public DateTime? QuickLoginPinValidUntil { get; set; }

        [AllowNull, Column(Order = 84), Display(Name = "Mobile Pin")]
        public string? MobilePin { get; set; }

        [AllowNull, Column(TypeName = "text", Order = 85), JsonIgnore]
        public string? SecurityStamp { get; set; }

        [AllowNull, Column(TypeName = "text", Order = 86), JsonIgnore]
        public string? ConcurrencyStamp { get; set; }

        [AllowNull, Column(TypeName = "boolean", Order = 87), DefaultValue(true)]
        public bool UserMustChangePassword { get; set; } = false;

        #region Retrieve Password
        [AllowNull, Display(Name = "Question for Password reset"), Column(Order = 88)]
        public string? PasswordQuestion { get; set; }

        [AllowNull, Display(Name = "Answer for password reset"), Column(Order = 89)]
        /// <summary>
        /// Password is Hashed ... 
        /// </summary>
        public string? PasswordAnswer { get; set; }
        #endregion

        [Required, NotNull, Column(Order = 90), Display(Name = "Is two factor enabled ?"), DefaultValue(false)]
        public bool TwoFactorEnabled { get; set; } = false;

        [AllowNull, Display(Name = "Account Valid Until"), Column(Order = 91)]
        public DateTime? AccountValidUntil { get; set; }

        [AllowNull, Display(Name = "Password Valid Until"), Column(Order = 92)]
        public DateTime? PasswordValidUntil { get; set; }

        [AllowNull, Display(Name = "Password Reset Token"), Column(Order = 93), JsonIgnore]
        public string? PasswordResetToken { get; set; }

        [AllowNull, Display(Name = "Password Reset Token Valid Until"), Column(Order = 94), JsonIgnore]
        public DateTime? PasswordResetTokenValidUntil { get; set; }

        #endregion

        #region Account Status
        [AllowNull, Display(Name = "Lockout Date"), DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}"), Column(Order = 100)]
        public DateTime? LockoutEnd { get; set; }

        [Required, NotNull, Column(Order = 101), Display(Name = "Is User LockedOut ?"), DefaultValue(false)]
        public bool LockoutEnabled { get; set; } = false;

        [Required, NotNull, Column(Order = 103), Display(Name = "Is user Approved ?"), DefaultValue(false)]
        public bool IsApproved { get; set; } = false; // Da cambiare se approvato di default

        [Required, NotNull, Column(Order = 104), Display(Name = "Number of Failed Access"), DefaultValue(0)]
        public int AccessFailedCount { get; set; } = 0;

        [Required, NotNull, Column(Order = 105), Display(Name = "Failed Password Attempt"), DefaultValue(0)]
        public int FailedPasswordAnswerAttemptCount { get; set; } = 0;
        #endregion

        #region Tracking
        [AllowNull, Display(Name = "Last web login date"), DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}"), Column(Order = 110)]
        public DateTime? LastLoginDate { get; set; }

        [AllowNull, Display(Name = "Last app login date"), DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}"), Column(Order = 111)]
        public DateTime? LastAppLoginDate { get; set; }

        [AllowNull, Column(Order = 112), Display(Name = "IP Address")]
        public string? IpAddress { get; set; }

        /// <summary>
        /// JSON o stringa
        /// </summary>
        [AllowNull, Column(Order = 113), Display(Name = "Last Known Location (Latitude, Longitude)")]
        public string? LastKnownLocation { get; set; } // 
        #endregion

        #region Deletititon
        [Required, NotNull, Column(Order = 802), Display(Name = "Is Customer deleted ?"), DefaultValue(false)]
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

        #region Navigation Properties
        /// <summary>
        /// Navigazione a claims dell'utente
        /// </summary>
        public virtual IEnumerable<AspNetUserClaim>? AspNetUserClaims { get; set; }

        /// <summary>
        /// Navigazione a claims dell'utente
        /// </summary>
        //public virtual IEnumerable<AspNetRole>? AspNetUserRoles { get; set; }

        #endregion
    }
}