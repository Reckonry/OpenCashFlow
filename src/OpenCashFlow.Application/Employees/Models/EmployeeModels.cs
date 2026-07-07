namespace OpenCashFlow.Application.Employees.Models;

public sealed class EmployeeRolePermissionResult
{
    public int Id { get; set; }
    public Guid RoleId { get; set; }
    public string? Permission { get; set; }
}

public sealed class EmployeeRoleResult
{
    public Guid RoleID { get; set; }
    public required string RoleName { get; set; }
    public string? NormalizedName { get; set; }
    public string? ConcurrencyStamp { get; set; }
    public IReadOnlyList<EmployeeRolePermissionResult> RolePermissions { get; set; } = [];
    public string? RoleImage { get; set; }
    public bool IsVisible { get; set; }
    public bool IsDeleted { get; set; }
    public Guid? IsDeletedBy { get; set; }
    public string? IsDeletedWhy { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime DateIns { get; set; }
    public Guid? EditedBy { get; set; }
    public DateTime? DateEdit { get; set; }
}

public sealed class EmployeeUserPermissionResult
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Permission { get; set; }
}

public sealed class EmployeeUserDeniedPermissionResult
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Permission { get; set; }
}

public class EmployeeListItem
{
    public Guid TenantID { get; set; }
    public Guid UserID { get; set; }
    public required string UserName { get; set; }
    public string? UserAvatar { get; set; }
    public string Language { get; set; } = "it";
    public string Country { get; set; } = "IT";
    public string? TimezoneID { get; set; }
    public IReadOnlyList<EmployeeRoleResult> Roles { get; set; } = [];
    public IReadOnlyList<EmployeeUserPermissionResult> AssignedPermissions { get; set; } = [];
    public IReadOnlyList<EmployeeUserDeniedPermissionResult> DeniedPermissions { get; set; } = [];
    public string? UserTitle { get; set; }
    public required string UserFirstName { get; set; }
    public string? UserMiddleName { get; set; }
    public string? UserLastName { get; set; }
    public required string Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public string? PhoneNumberPrefix { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public string? Gender { get; set; }
    public string? Pronouns { get; set; }
    public bool PrivacyPolicyAcepted { get; set; }
    public string? PrivacyPolicyVersion { get; set; }
    public DateTime? PrivacyPolicyAcceptedDate { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; }
    public bool IsApproved { get; set; }
    public int AccessFailedCount { get; set; }
    public int FailedPasswordAnswerAttemptCount { get; set; }
    public double? TimeCost { get; set; }
    public string? BadgeID { get; set; }
    public bool OutOfReports { get; set; }
    public bool RequireShiftCheckIn { get; set; }
    public DateTime? LastCheckIn { get; set; }
    public DateTime? LastCheckOut { get; set; }
    public string? Role { get; set; }
    public string? Department { get; set; }
    public string? WorkLocation { get; set; }
    public string? AccessLevel { get; set; }
    public string? AuthorizedAreas { get; set; }
}

public sealed class EmployeeDetailResult : EmployeeListItem
{
    public string? Timezone { get; set; }
    public DateOnly? DoB { get; set; }
    public string? PoB { get; set; }
    public string? SoB { get; set; }
    public string? CoB { get; set; }
    public string? Nationality { get; set; }
    public DateTime? ContractStartDate { get; set; }
    public DateTime? ContractEndDate { get; set; }
    public decimal? MonthlySalary { get; set; }
    public decimal? Bonuses { get; set; }
    public decimal? Allowances { get; set; }
    public string? EmploymentType { get; set; }
    public decimal? OvertimeRate { get; set; }
    public string? Skills { get; set; }
    public Guid? SupervisorID { get; set; }
    public string? InternalNotes { get; set; }
    public string? PublicNotes { get; set; }
    public string? ExternalSystemReference { get; set; }
    public string? SyncStatus { get; set; }
    public bool IsDeleted { get; set; }
    public Guid? IsDeletedBy { get; set; }
    public string? IsDeletedWhy { get; set; }
    public DateTime? DateDeleted { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime DateIns { get; set; }
    public Guid? EditedBy { get; set; }
    public DateTime? DateEdit { get; set; }
}

public sealed class EmployeeProfileUpdate
{
    public string UserFirstName { get; set; } = string.Empty;
    public string? UserLastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumberPrefix { get; set; }
    public string? PhoneNumber { get; set; }
    public string Language { get; set; } = "it";
    public string Country { get; set; } = "IT";
    public string? Timezone { get; set; }
}

public class EmployeeWriteDraft
{
    public Guid TenantID { get; set; }
    public Guid UserID { get; set; }
    public required string UserName { get; set; }
    public string? UserAvatar { get; set; }
    public string Language { get; set; } = "it";
    public string Country { get; set; } = "IT";
    public string? Timezone { get; set; }
    public Guid? SelectedRoleID { get; set; }
    public IReadOnlyCollection<Guid> RoleIDs { get; set; } = [];
    public string? UserTitle { get; set; }
    public required string UserFirstName { get; set; }
    public string? UserMiddleName { get; set; }
    public string? UserLastName { get; set; }
    public required string Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public string? PhoneNumberPrefix { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public string? Gender { get; set; }
    public string? Pronouns { get; set; }
    public DateOnly? DoB { get; set; }
    public string? PoB { get; set; }
    public string? SoB { get; set; }
    public string? CoB { get; set; }
    public string? Nationality { get; set; }
    public bool PrivacyPolicyAcepted { get; set; }
    public string? PrivacyPolicyVersion { get; set; }
    public DateTime? PrivacyPolicyAcceptedDate { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; }
    public bool IsApproved { get; set; }
    public int AccessFailedCount { get; set; }
    public int FailedPasswordAnswerAttemptCount { get; set; }
    public double? TimeCost { get; set; }
    public string? BadgeID { get; set; }
    public bool OutOfReports { get; set; }
    public bool RequireShiftCheckIn { get; set; } = true;
    public DateTime? LastCheckIn { get; set; }
    public DateTime? LastCheckOut { get; set; }
    public string? Role { get; set; }
    public string? Department { get; set; }
    public string? WorkLocation { get; set; }
    public DateTime? ContractStartDate { get; set; }
    public DateTime? ContractEndDate { get; set; }
    public decimal? MonthlySalary { get; set; }
    public decimal? Bonuses { get; set; }
    public decimal? Allowances { get; set; }
    public string? EmploymentType { get; set; }
    public decimal? OvertimeRate { get; set; }
    public string? Skills { get; set; }
    public Guid? SupervisorID { get; set; }
    public string? AccessLevel { get; set; }
    public string? AuthorizedAreas { get; set; }
    public string? InternalNotes { get; set; }
    public string? PublicNotes { get; set; }
    public string? ExternalSystemReference { get; set; }
    public string? SyncStatus { get; set; }
    public bool IsDeleted { get; set; }
    public Guid? IsDeletedBy { get; set; }
    public string? IsDeletedWhy { get; set; }
    public DateTime? DateDeleted { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime DateIns { get; set; } = DateTime.UtcNow;
    public Guid? EditedBy { get; set; }
    public DateTime? DateEdit { get; set; }
    public required string PasswordHash { get; set; }
    public required string PasswordSalt { get; set; }
    public string? QuickLoginPinHash { get; set; }
    public bool UserMustChangePassword { get; set; }
    public string? PasswordQuestion { get; set; }
    public string? PasswordAnswer { get; set; }
}

public sealed class EmployeeCredentialSnapshot
{
    public Guid TenantID { get; set; }
    public Guid UserID { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required string UserFirstName { get; set; }
    public string? UserLastName { get; set; }
    public string PasswordSalt { get; set; } = string.Empty;
}
