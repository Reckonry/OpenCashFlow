using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Shared.Models.Identity;

namespace Shared.DTOs.Employees
{
    public class Employee_List_DTO
    {
        #region Data Linking
        [Required, NotNull]
        public Guid TenantID { get; set; } // Link to the company
        #endregion

        #region Employee Identification
        [Required, NotNull, Key]
        public Guid UserID { get; set; } // Link to AspNetUsers
        #endregion

        [Required, NotNull, Display(Name = "Username")]
        public required string UserName { get; set; }

        [Required, Display(Name = "Normalized Username"), JsonIgnore]
        public string? NormalizedUserName { get => Regex.Replace(UserName, "[^0-9a-zA-Z@_.-]+", "_").ToLower(); }

        #region User Customization Property
        [AllowNull, Display(Name = "User Avatar")]
        public string? UserAvatar { get; set; }

        [Required, NotNull, Display(Name = "Language")]
        public string Language { get; set; } = "it";

        [Required, NotNull, Display(Name = "Country")]
        public string Country { get; set; } = "IT";

        [AllowNull, Display(Name = "Timezone")]
        public string? TimezoneID { get; set; }
        #endregion

        #region User privileges
        /// <summary> connected roles applied to the user. </summary>
        public ICollection<AspNetRole> Roles { get; set; } = new List<AspNetRole>();

        /// <summary> Allowed privileges </summary>
        public ICollection<AspNetUserPermission> AssignedPermissions { get; set; } = new List<AspNetUserPermission>();

        /// <summary> Denied privileges </summary>
        public ICollection<AspNetUserDeniedPermission> DeniedPermissions { get; set; } = new List<AspNetUserDeniedPermission>();

        /// <summary> Checks whether the user has a permission, taking denials into account. </summary>
        public bool HasPermission(string permission)
        {
            // If the permission is denied, the user cannot perform it
            if (DeniedPermissions.Any(p => p.Permission == permission))
                return false;

            // If the permission is assigned directly, the user can perform it
            if (AssignedPermissions.Any(p => p.Permission == permission))
                return true;

            // If the permission is present in one of the assigned roles, the user can perform it
            return Roles?.SelectMany(r => r.RolePermissions).Any(p => p.Permission == permission) ?? false;
        }
        #endregion

        #region User Personal details
        #region Name - Surname
        [AllowNull, Display(Name = "User Title")]
        public string? UserTitle { get; set; }

        [Required(ErrorMessage = "Provide a first name"), NotNull, Display(Name = "First name")]
        public required string UserFirstName { get; set; }

        [AllowNull, Display(Name = "Middlename"), ]
        public string? UserMiddleName { get; set; }

        [AllowNull, Display(Name = "Surname")]
        public string? UserLastName { get; set; }

        [NotMapped, Display(Name = "User Name Surname")]
        public string? EmployeeNameSurname { get => UserTitle + " " + UserFirstName + " " + UserLastName; }
        [NotMapped, Display(Name = "User Surname Name")]
        public string? EmployeeSurnameName { get => UserTitle + " " + UserLastName + " " + UserFirstName; }
        #endregion

        #region Email and phone
        [Required, NotNull, Display(Name = "E-Mail")]
        [EmailAddress(ErrorMessage = "E-mail address not valid")]
        public required string Email { get; set; }

        public string? NormalizedEmail => !string.IsNullOrEmpty(Email)
            ? Regex.Replace(Email, "[^0-9a-zA-Z]+", "_")
            : null;

        [Required, Display(Name = "Is Email Confirmed ?")]
        public bool EmailConfirmed { get; set; } = false;

        [AllowNull, Display(Name = "Mobile Country Code Prefix")]
        public string? PhoneNumberPrefix { get; set; }

        [AllowNull,  Display(Name = "Mobile Number")]
        public string? PhoneNumber { get; set; }

        [Required, NotNull, Display(Name = "Is mobile phone confirmed ?")]
        public bool PhoneNumberConfirmed { get; set; } = false;
        #endregion

        #region GenZ stuff
        /// <summary>
        /// Based on the enum GenderTypes
        /// </summary>
        [AllowNull, Display(Name = "Gender")]
        public string? Gender { get; set; }

        /// <summary>
        /// Based on the enum PronounsTypes
        /// </summary>
        [AllowNull, Display(Name = "Pronouns")]
        public string? Pronouns { get; set; }
        #endregion

        #region User Accepted Terms & Conditions
        [Required, Display(Name = "Accepts Privacy Policy")]
        public bool PrivacyPolicyAcepted { get; set; } = false;

        [AllowNull, Display(Name = "Accepts Privacy Policy")]
        public string? PrivacyPolicyVersion { get; set; }

        [AllowNull, Display(Name = "Privacy Policy Accepted Date")]
        public DateTime? PrivacyPolicyAcceptedDate { get; set; }
        #endregion

        #region Account Status
        [AllowNull, Display(Name = "Lockout Date")]
        public DateTime? LockoutEnd { get; set; }

        [Required, NotNull, Display(Name = "Is User LockedOut ?")]
        public bool LockoutEnabled { get; set; } = false;

        [Required, NotNull, Display(Name = "Is user Approved ?")]
        public bool IsApproved { get; set; } = false; // Da cambiare se approvato di default

        [Required, NotNull, Display(Name = "Number of Failed Access")]
        public int AccessFailedCount { get; set; } = 0;

        [Required, NotNull, Display(Name = "Failed Password Attempt")]
        public int FailedPasswordAnswerAttemptCount { get; set; } = 0;
        #endregion

        #endregion
        #region Operational Details
        [AllowNull, Display(Name = "Employee Cost per Hour")]
        public double? TimeCost { get; set; } // Costo orario del dipendente

        [AllowNull, Display(Name = "Badge ID")]
        public string? BadgeID { get; set; } // ID del badge per accesso

        [Required, NotNull, Display(Name = "Is User Out of Reports?")]
        public bool OutOfReports { get; set; } = false; // Esclusione dai report

        [Required, NotNull, Display(Name = "Is User Required to Check In?")]
        public bool RequireShiftCheckIn { get; set; } = true; // Obbligo di timbratura

        [AllowNull, Display(Name = "Last Check In")]
        public DateTime? LastCheckIn { get; set; } // Ultimo check-in

        [AllowNull, Display(Name = "Last Check Out")]
        public DateTime? LastCheckOut { get; set; } // Ultimo check-out
        #endregion

        #region Personal and Role Details
        [AllowNull, Display(Name = "Role")]
        public string? Role { get; set; } // Ruolo del dipendente (Es: Manager, Tecnico, etc.)

        [AllowNull, Display(Name = "Department")]
        public string? Department { get; set; } // Dipartimento di appartenenza

        [AllowNull, Display(Name = "Work Location")]
        public string? WorkLocation { get; set; } // Sede di lavoro
        #endregion


        #region Access Control
        [AllowNull, Display(Name = "Access Level")]
        public string? AccessLevel { get; set; } // Livello di accesso agli edifici o strumenti

        [AllowNull, Display(Name = "Authorized Areas")]
        public string? AuthorizedAreas { get; set; } // Aree autorizzate (JSON o CSV)
        #endregion

     

    }
}
