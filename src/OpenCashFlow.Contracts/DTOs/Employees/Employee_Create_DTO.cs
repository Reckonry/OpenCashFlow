using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace OpenCashFlow.Contracts.DTOs.Employees
{
    public partial class Employee_Create_DTO
    {
        public Employee_Create_DTO()
        {
            UserName = string.Empty;
            UserFirstName = string.Empty;
            Email = string.Empty;
        }

        #region Data Linking
        public Guid TenantID { get; set; } // Link to the company
        #endregion

        #region Employee Identification
        public Guid UserID { get; set; } // Link to AspNetUsers
        #endregion

        public string UserName { get; set; } = string.Empty;

        public string? NormalizedUserName { get => MyRegex().Replace(UserName, "_").ToLower(); }

        #region User Customization Property
        public string? UserAvatar { get; set; }

        public string Language { get; set; } = "it";

        public string Country { get; set; } = "IT";

        public string? Timezone { get; set; }
        #endregion

        // Single role selection during creation (optional)
        public Guid? SelectedRoleID { get; set; }

        #region User privileges
        /// <summary> Allowed privileges </summary>
        public ICollection<EmployeeUserPermissionDto> AssignedPermissions { get; set; } = [];

        /// <summary> Denied privileges </summary>
        public ICollection<EmployeeUserDeniedPermissionDto> DeniedPermissions { get; set; } = [];

        /// <summary> Checks whether the user has a permission, taking denials into account. </summary>
        //public bool HasPermission(string permission)
        //{
        //    // If the permission is denied, the user cannot perform it
        //    if (DeniedPermissions.Any(p => p.Permission == permission))
        //        return false;

        //    // If the permission is assigned directly, the user can perform it
        //    if (AssignedPermissions.Any(p => p.Permission == permission))
        //        return true;

        //    // If the permission is present in one of the assigned roles, the user can perform it
        //    return Roles?.SelectMany(r => r.RolePermissions).Any(p => p.Permission == permission) ?? false;
        //}
        #endregion

        #region User Personal details
        #region Name - Surname
        public string? UserTitle { get; set; }

        [Required(ErrorMessage = "Provide a first name"), NotNull, Display(Name = "First name")]
        public required string UserFirstName { get; set; } = string.Empty;

        [AllowNull, Display(Name = "Middle name"),]
        public string? UserMiddleName { get; set; }

        [AllowNull, Display(Name = "Last name")]
        public string? UserLastName { get; set; }

        [Display(Name = "User Name Surname")]
        public string? EmployeeNameSurname { get => UserTitle + " " + UserFirstName + " " + UserLastName; }
        [Display(Name = "User Surname Name")]
        public string? EmployeeSurnameName { get => UserTitle + " " + UserLastName + " " + UserFirstName; }
        #endregion

        #region Email and phone
        [Required, NotNull, Display(Name = "E-Mail")]
        [EmailAddress(ErrorMessage = "E-mail address not valid")]
        public required string Email { get; set; } = string.Empty;

        public string? NormalizedEmail { get => Regex.Replace(Email, "[^0-9a-zA-Z]+", "_"); }

        [Required, Display(Name = "Is Email Confirmed ?")]
        public bool EmailConfirmed { get; set; } = false;

        [AllowNull, Display(Name = "Mobile Country Code Prefix")]
        public string? PhoneNumberPrefix { get; set; }

        [AllowNull, Display(Name = "Mobile Number")]
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

        #region Birth & nationality
        [AllowNull, Display(Name = "Date of Birth")]
        public DateOnly? DoB { get; set; }

        [AllowNull, Display(Name = "City of Birth")]
        public string? PoB { get; set; }

        [AllowNull, Display(Name = "State of Birth")]
        public string? SoB { get; set; }

        [AllowNull, Display(Name = "Country of Birth")]
        public string? CoB { get; set; }

        [AllowNull, Display(Name = "Nationality")]
        public string? Nationality { get; set; }
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

        #region Employment Details
        [AllowNull, Display(Name = "Contract Start Date")]
        public DateTime? ContractStartDate { get; set; } // Data di inizio contratto

        [AllowNull, Display(Name = "Contract End Date")]
        public DateTime? ContractEndDate { get; set; } // Data di fine contratto

        [AllowNull, Display(Name = "Monthly Salary")]
        public decimal? MonthlySalary { get; set; } // Salario mensile

        [AllowNull, Display(Name = "Bonuses")]
        public decimal? Bonuses { get; set; } // Bonus annuali o straordinari

        [AllowNull, Display(Name = "Allowances")]
        public decimal? Allowances { get; set; } // Benefici extra (es. buoni pasto)
        #endregion

        #region Additional Attributes
        [AllowNull, Display(Name = "Employment Type")]
        public string? EmploymentType { get; set; } // Tipo di contratto (Es: Full-Time, Part-Time, Freelance)

        [AllowNull, Display(Name = "Overtime Rate")]
        public decimal? OvertimeRate { get; set; } // Tariffa straordinari

        [AllowNull, Display(Name = "Skills")]
        /// <summary>
        /// Competenze formattate come JSON o CSV
        /// </summary>
        public string? Skills { get; set; }

        [AllowNull, Display(Name = "Supervisor ID")]
        /// <summary>
        /// ID del supervisore
        /// </summary>
        public Guid? SupervisorID { get; set; }
        #endregion

        #region Security & Password
        [Display(Name = "Current Password")]
        public string? TmpCurrentPassword { get; set; }

        [Required, Display(Name = "New Password")]
        public string? TmpNewPassword { get; set; }

        [Required, Compare("TmpNewPassword", ErrorMessage = "Passwords do not match"), Display(Name = "Repeat New Password")]
        public string? TmpNewPasswordRepeat { get; set; }

        [Display(Name = "Fast Login Pin")]
        public string? TmpFastLoginPin { get; set; }

        [Display(Name = "Password Question")]
        public string? TmpPasswordQuestion { get; set; }

        [Display(Name = "Password Answer")]
        public string? TmpPasswordAnswer { get; set; }
        #endregion

        #region Access Control
        [AllowNull, Display(Name = "Access Level")]
        public string? AccessLevel { get; set; } // Livello di accesso agli edifici o strumenti

        [AllowNull, Display(Name = "Authorized Areas")]
        public string? AuthorizedAreas { get; set; } // Aree autorizzate (JSON o CSV)
        #endregion

        #region Additional Attributes
        [AllowNull, Display(Name = "Internal Notes")]
        public string? InternalNotes { get; set; } // Note interne

        [AllowNull, Display(Name = "Public Notes")]
        public string? PublicNotes { get; set; } // Note pubbliche visibili nei report

        [AllowNull, Display(Name = "External System Reference")]
        public string? ExternalSystemReference { get; set; } // Collegamenti a sistemi esterni

        [AllowNull, Display(Name = "Sync Status")]
        public string? SyncStatus { get; set; } // Stato di sincronizzazione
        #endregion

        #region Deletititon
        [Required, NotNull, Display(Name = "Is deleted ?")]
        public bool IsDeleted { get; set; } = false;

        [AllowNull, Display(Name = "Who deleted this ?")]
        public Guid? IsDeletedBy { get; set; }

        [AllowNull, Display(Name = "Why is deleted ?")]
        public string? IsDeletedWhy { get; set; }

        [AllowNull, Display(Name = "Date deleted")]
        public DateTime? DateDeleted { get; set; }
        #endregion

        #region Audit & Tracking
        [AllowNull, Display(Name = "Created By")]
        public Guid? CreatedBy { get; set; }

        [Required, NotNull, Display(Name = "Created")]
        public DateTime DateIns { get; set; } = DateTime.UtcNow;

        [AllowNull, Display(Name = "Edited By")]
        public Guid? EditedBy { get; set; }

        [AllowNull, Display(Name = "Date edit")]
        public DateTime? DateEdit { get; set; }

        [GeneratedRegex("[^0-9a-zA-Z@_.-]+")]
        private static partial Regex MyRegex();
        #endregion

    }
}
