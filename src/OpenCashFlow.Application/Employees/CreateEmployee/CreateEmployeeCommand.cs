namespace OpenCashFlow.Application.Employees.CreateEmployee;

public sealed class CreateEmployeeCommand
{
    public Guid TenantID { get; set; }
    public Guid CurrentUserID { get; set; }
    public Guid? SelectedRoleID { get; set; }
    public string? UserAvatar { get; set; }
    public string Language { get; set; } = "it";
    public string Country { get; set; } = "IT";
    public string? Timezone { get; set; }
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
    public string? PasswordQuestion { get; set; }
    public string? PasswordAnswer { get; set; }
    public string? AccessLevel { get; set; }
    public string? AuthorizedAreas { get; set; }
    public string? InternalNotes { get; set; }
    public string? PublicNotes { get; set; }
    public string? ExternalSystemReference { get; set; }
    public string? SyncStatus { get; set; }
    public string? NewPassword { get; set; }
}
