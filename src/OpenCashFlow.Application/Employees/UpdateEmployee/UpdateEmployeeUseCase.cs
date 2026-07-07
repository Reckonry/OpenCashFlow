using OpenCashFlow.Application.Employees.Models;
using OpenCashFlow.Application.Employees.Ports;

namespace OpenCashFlow.Application.Employees.UpdateEmployee;

public sealed class UpdateEmployeeUseCase(
    IEmployeeReader employeeReader,
    IEmployeeWriter employeeWriter,
    IEmployeeCredentialService credentialService,
    IEmployeePinService pinService,
    IEmployeeNotificationSender notificationSender) : IUpdateEmployeeUseCase
{
    public async Task<UpdateEmployeeResult> ExecuteAsync(UpdateEmployeeCommand command, CancellationToken cancellationToken = default)
    {
        if (command.TenantID == Guid.Empty) throw new ArgumentException("Tenant id is required", nameof(command.TenantID));
        if (command.CurrentUserID == Guid.Empty) throw new ArgumentException("Current user id is required", nameof(command.CurrentUserID));
        if (command.UserID == Guid.Empty) throw new ArgumentException("User id is required", nameof(command.UserID));

        var existing = await employeeReader.GetEmployeeCredentialAsync(command.UserID, command.TenantID, cancellationToken);
        if (existing is null)
        {
            return new UpdateEmployeeResult(false, false, false, "Employee not found", null);
        }

        var emailChanged = !string.IsNullOrWhiteSpace(command.Email) &&
            !string.Equals(existing.Email, command.Email, StringComparison.OrdinalIgnoreCase);

        if (emailChanged)
        {
            if (!IsValidEmail(command.Email))
            {
                throw new ArgumentException("Invalid email format", nameof(command.Email));
            }

            if (await employeeReader.EmailInUseByOtherUserAsync(command.Email, command.UserID, cancellationToken))
            {
                throw new InvalidOperationException("Email already used by another employee");
            }

            command.EmailConfirmed = true;
            command.IsApproved = true;
        }

        var updated = await employeeWriter.UpdateAsync(ToDraft(command, existing.PasswordSalt), cancellationToken);
        if (!updated)
        {
            return new UpdateEmployeeResult(false, false, false, "Employee not found", null);
        }

        var pinSent = true;
        if (emailChanged)
        {
            pinSent = await TrySendPinToChangedEmailAsync(command, existing, cancellationToken);
        }

        var message = emailChanged
            ? pinSent
                ? $"Employee updated successfully. PIN sent to the new email address: {command.Email}"
                : $"Employee updated successfully, but there was an error sending the PIN to the new email address: {command.Email}"
            : "Employee updated successfully";

        return new UpdateEmployeeResult(true, emailChanged, emailChanged ? pinSent : true, message, emailChanged ? command.Email : null);
    }

    private async Task<bool> TrySendPinToChangedEmailAsync(UpdateEmployeeCommand command, EmployeeCredentialSnapshot existing, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(existing.PasswordSalt))
            {
                throw new InvalidOperationException("Missing PasswordSalt - unable to generate a new PIN");
            }

            var newPin = await pinService.GenerateUniquePinAsync(command.TenantID, cancellationToken);
            await employeeWriter.UpdatePinHashAsync(command.UserID, credentialService.HashSecret(newPin, existing.PasswordSalt), cancellationToken);

            var updatedEmployee = new EmployeeCredentialSnapshot
            {
                TenantID = existing.TenantID,
                UserID = existing.UserID,
                UserName = existing.UserName,
                Email = command.Email,
                UserFirstName = command.UserFirstName,
                UserLastName = command.UserLastName,
                PasswordSalt = existing.PasswordSalt
            };
            await notificationSender.SendEmployeePinChangedAsync(new EmployeePinNotification(updatedEmployee, command.Email, newPin), cancellationToken);

            if (!string.IsNullOrWhiteSpace(existing.Email) && !string.Equals(existing.Email, command.Email, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    await notificationSender.SendEmployeeEmailChangedAsync(new EmployeeEmailChangedNotification(updatedEmployee, existing.Email, command.Email), cancellationToken);
                }
                catch
                {
                    // Notification to old email is informational and must not fail the update.
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static EmployeeWriteDraft ToDraft(UpdateEmployeeCommand command, string passwordSalt)
    {
        return new EmployeeWriteDraft
        {
            TenantID = command.TenantID,
            UserID = command.UserID,
            UserName = command.UserName,
            UserAvatar = command.UserAvatar,
            Language = command.Language,
            Country = command.Country,
            Timezone = command.Timezone,
            SelectedRoleID = command.SelectedRoleID,
            RoleIDs = command.RoleIDs,
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
            IsApproved = command.IsApproved,
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
            AccessLevel = command.AccessLevel,
            AuthorizedAreas = command.AuthorizedAreas,
            InternalNotes = command.InternalNotes,
            PublicNotes = command.PublicNotes,
            ExternalSystemReference = command.ExternalSystemReference,
            SyncStatus = command.SyncStatus,
            IsDeleted = command.IsDeleted,
            IsDeletedBy = command.IsDeletedBy,
            IsDeletedWhy = command.IsDeletedWhy,
            DateDeleted = command.DateDeleted,
            CreatedBy = command.CreatedBy,
            DateIns = command.DateIns,
            EditedBy = command.CurrentUserID,
            DateEdit = DateTime.UtcNow,
            PasswordSalt = passwordSalt,
            PasswordHash = string.Empty
        };
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
