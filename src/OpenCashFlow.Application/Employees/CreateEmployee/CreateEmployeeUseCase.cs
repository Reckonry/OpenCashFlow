using OpenCashFlow.Application.Employees.Models;
using OpenCashFlow.Application.Employees.Ports;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenCashFlow.Application.Employees.CreateEmployee;

public sealed class CreateEmployeeUseCase(
    IEmployeeReader employeeReader,
    IEmployeeWriter employeeWriter,
    IEmployeeCredentialService credentialService,
    IEmployeePinService pinService,
    IEmployeeNotificationSender notificationSender) : ICreateEmployeeUseCase
{
    private const int PolicyMaxUserNameLength = 40;
    private const int DbMaxUserNameLength = 256;
    private static readonly int MaxUserNameLength = Math.Min(PolicyMaxUserNameLength, DbMaxUserNameLength);
    private static readonly Regex AllowedUsernameCharsRegex = new("[^a-z0-9._-]+", RegexOptions.Compiled);
    private static readonly Regex RepeatSeparatorsRegex = new("[._-]{2,}", RegexOptions.Compiled);
    private static readonly Regex EdgeSeparatorsRegex = new("^[._-]+|[._-]+$", RegexOptions.Compiled);

    public async Task<CreateEmployeeResult> ExecuteAsync(CreateEmployeeCommand command, CancellationToken cancellationToken = default)
    {
        if (command.TenantID == Guid.Empty) throw new ArgumentException("Tenant id is required", nameof(command.TenantID));
        if (command.CurrentUserID == Guid.Empty) throw new ArgumentException("Current user id is required", nameof(command.CurrentUserID));
        if (string.IsNullOrWhiteSpace(command.Email)) throw new ArgumentException("Email is required", nameof(command.Email));
        if (string.IsNullOrWhiteSpace(command.NewPassword)) throw new ArgumentException("Password is required", nameof(command.NewPassword));
        if (!credentialService.IsStrongPassword(command.NewPassword))
        {
            throw new ArgumentException("Password must be at least 8 characters and include an uppercase letter, a lowercase letter, a number, and a special character.", nameof(command.NewPassword));
        }

        if (await employeeReader.EmailExistsAsync(command.Email, cancellationToken))
        {
            throw new InvalidOperationException("Email already exists");
        }

        var userId = Guid.NewGuid();
        var username = GenerateUsername(command, userId);
        var pin = await pinService.GenerateUniquePinAsync(command.TenantID, cancellationToken);
        var salt = credentialService.GenerateSalt();
        var employee = await employeeWriter.CreateAsync(ToDraft(command, userId, username, salt, credentialService.HashSecret(command.NewPassword, salt), credentialService.HashSecret(pin, salt)), cancellationToken)
            ?? throw new InvalidOperationException("Employee retrieval failed after creation.");

        try
        {
            await notificationSender.SendEmployeeCreatedPinAsync(new EmployeePinNotification(
                ToCredentialSnapshot(command.TenantID, employee, salt),
                command.Email,
                pin), cancellationToken);
        }
        catch
        {
            // Email delivery is optional and must not block self-hosted employee creation.
        }

        return new CreateEmployeeResult(employee);
    }

    private static EmployeeWriteDraft ToDraft(CreateEmployeeCommand command, Guid userId, string username, string salt, string passwordHash, string pinHash)
    {
        return new EmployeeWriteDraft
        {
            TenantID = command.TenantID,
            UserID = userId,
            UserName = username,
            UserAvatar = command.UserAvatar,
            Language = command.Language,
            Country = command.Country,
            Timezone = command.Timezone,
            SelectedRoleID = command.SelectedRoleID,
            UserTitle = command.UserTitle,
            UserFirstName = command.UserFirstName,
            UserMiddleName = command.UserMiddleName,
            UserLastName = command.UserLastName,
            Email = command.Email,
            EmailConfirmed = command.EmailConfirmed,
            PhoneNumberPrefix = command.PhoneNumberPrefix,
            PhoneNumber = command.PhoneNumber,
            PhoneNumberConfirmed = command.PhoneNumberConfirmed,
            Gender = command.Gender,
            Pronouns = command.Pronouns,
            DoB = command.DoB,
            PoB = command.PoB,
            SoB = command.SoB,
            CoB = command.CoB,
            Nationality = command.Nationality,
            PrivacyPolicyAcepted = command.PrivacyPolicyAcepted,
            PrivacyPolicyVersion = command.PrivacyPolicyVersion,
            PrivacyPolicyAcceptedDate = command.PrivacyPolicyAcceptedDate,
            LockoutEnd = command.LockoutEnd,
            LockoutEnabled = command.LockoutEnabled,
            IsApproved = true,
            AccessFailedCount = command.AccessFailedCount,
            FailedPasswordAnswerAttemptCount = command.FailedPasswordAnswerAttemptCount,
            TimeCost = command.TimeCost,
            BadgeID = command.BadgeID,
            OutOfReports = command.OutOfReports,
            RequireShiftCheckIn = command.RequireShiftCheckIn,
            LastCheckIn = command.LastCheckIn,
            LastCheckOut = command.LastCheckOut,
            Role = command.Role,
            Department = command.Department,
            WorkLocation = command.WorkLocation,
            ContractStartDate = command.ContractStartDate,
            ContractEndDate = command.ContractEndDate,
            MonthlySalary = command.MonthlySalary,
            Bonuses = command.Bonuses,
            Allowances = command.Allowances,
            EmploymentType = command.EmploymentType,
            OvertimeRate = command.OvertimeRate,
            Skills = command.Skills,
            SupervisorID = command.SupervisorID,
            PasswordQuestion = command.PasswordQuestion,
            PasswordAnswer = command.PasswordAnswer,
            AccessLevel = command.AccessLevel,
            AuthorizedAreas = command.AuthorizedAreas,
            InternalNotes = command.InternalNotes,
            PublicNotes = command.PublicNotes,
            ExternalSystemReference = command.ExternalSystemReference,
            SyncStatus = command.SyncStatus,
            CreatedBy = command.CurrentUserID,
            DateIns = DateTime.UtcNow,
            PasswordSalt = salt,
            PasswordHash = passwordHash,
            QuickLoginPinHash = pinHash,
            UserMustChangePassword = true
        };
    }

    private static EmployeeCredentialSnapshot ToCredentialSnapshot(Guid tenantId, EmployeeDetailResult employee, string salt)
    {
        return new EmployeeCredentialSnapshot
        {
            TenantID = tenantId,
            UserID = employee.UserID,
            UserName = employee.UserName,
            Email = employee.Email,
            UserFirstName = employee.UserFirstName,
            UserLastName = employee.UserLastName,
            PasswordSalt = salt
        };
    }

    private static string GenerateUsername(CreateEmployeeCommand command, Guid userId)
    {
        var pieces = new[] { command.UserFirstName, command.UserLastName }
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s!.Trim());

        var seed = string.Join(".", pieces);
        if (string.IsNullOrWhiteSpace(seed) && !string.IsNullOrWhiteSpace(command.Email))
        {
            seed = command.Email.Split('@')[0];
        }

        seed = SanitizeUsername(string.IsNullOrWhiteSpace(seed) ? "user" : seed);
        if (string.IsNullOrWhiteSpace(seed)) seed = "user";

        var tag = userId.ToString("N")[..6].ToLowerInvariant();
        var head = seed.Length > MaxUserNameLength - 7 ? seed[..Math.Max(1, MaxUserNameLength - 7)] : seed;
        return $"{head}-{tag}";
    }

    private static string SanitizeUsername(string input)
    {
        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        var result = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        result = AllowedUsernameCharsRegex.Replace(result, string.Empty);
        result = RepeatSeparatorsRegex.Replace(result, m => m.Value[0].ToString());
        return EdgeSeparatorsRegex.Replace(result, string.Empty);
    }
}
