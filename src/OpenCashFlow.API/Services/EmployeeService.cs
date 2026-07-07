using OpenCashFlow.API.Services.Interfaces;
using OpenCashFlow.Application.Employees.CreateEmployee;
using OpenCashFlow.Application.Employees.DeleteEmployee;
using OpenCashFlow.Application.Employees.GetEmployeeDetail;
using OpenCashFlow.Application.Employees.GetEmployees;
using OpenCashFlow.Application.Employees.Models;
using OpenCashFlow.Application.Employees.ResendPin;
using OpenCashFlow.Application.Employees.UpdateEmployee;
using OpenCashFlow.Application.Employees.UpdateMyProfile;
using OpenCashFlow.Contracts.DTOs.Employees;

namespace OpenCashFlow.API.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IGetEmployeesUseCase _getEmployeesUseCase;
        private readonly IGetEmployeeDetailUseCase _getEmployeeDetailUseCase;
        private readonly ICreateEmployeeUseCase _createEmployeeUseCase;
        private readonly IUpdateEmployeeUseCase _updateEmployeeUseCase;
        private readonly IDeleteEmployeeUseCase _deleteEmployeeUseCase;
        private readonly IUpdateMyProfileUseCase _updateMyProfileUseCase;
        private readonly IResendEmployeePinUseCase _resendEmployeePinUseCase;

        public EmployeeService(
            IAuthenticationService authenticationService,
            IGetEmployeesUseCase getEmployeesUseCase,
            IGetEmployeeDetailUseCase getEmployeeDetailUseCase,
            ICreateEmployeeUseCase createEmployeeUseCase,
            IUpdateEmployeeUseCase updateEmployeeUseCase,
            IDeleteEmployeeUseCase deleteEmployeeUseCase,
            IUpdateMyProfileUseCase updateMyProfileUseCase,
            IResendEmployeePinUseCase resendEmployeePinUseCase)
        {
            _authenticationService = authenticationService;
            _getEmployeesUseCase = getEmployeesUseCase;
            _getEmployeeDetailUseCase = getEmployeeDetailUseCase;
            _createEmployeeUseCase = createEmployeeUseCase;
            _updateEmployeeUseCase = updateEmployeeUseCase;
            _deleteEmployeeUseCase = deleteEmployeeUseCase;
            _updateMyProfileUseCase = updateMyProfileUseCase;
            _resendEmployeePinUseCase = resendEmployeePinUseCase;
        }

        public async Task<IEnumerable<Employee_List_DTO>?> GetEmployeesAsync(CancellationToken cancellationToken)
        {
            var employees = await _getEmployeesUseCase.ExecuteAsync(_authenticationService.GetTenantID(), cancellationToken);
            return employees.Select(MapEmployeeList).ToList();
        }

        public async Task<Employee_Detail_DTO?> GetEmployeesByIDAsync(Guid UserID, CancellationToken cancellationToken)
        {
            var employee = await _getEmployeeDetailUseCase.ExecuteAsync(UserID, _authenticationService.GetTenantID(), cancellationToken);
            return employee is null ? null : MapEmployeeDetail(employee);
        }

        public async Task<Employee_Detail_DTO> CreateEmployeeAsync(Employee_Create_DTO model, CancellationToken cancellationToken)
        {
            var result = await _createEmployeeUseCase.ExecuteAsync(MapCreateCommand(model), cancellationToken);
            return MapEmployeeDetail(result.Employee);
        }

        public async Task<Employee_Update_Response_DTO> UpdateEmployeeAsync(Guid userID, Employee_Update_DTO model, CancellationToken cancellationToken)
        {
            var result = await _updateEmployeeUseCase.ExecuteAsync(MapUpdateCommand(userID, model), cancellationToken);
            return new Employee_Update_Response_DTO
            {
                Success = result.Success,
                EmailChanged = result.EmailChanged,
                PinSentSuccessfully = result.PinSentSuccessfully,
                Message = result.Message,
                NewEmail = result.NewEmail
            };
        }

        public async Task<bool> DeleteEmployeeAsync(Guid userID, CancellationToken cancellationToken)
        {
            return await _deleteEmployeeUseCase.ExecuteAsync(
                userID,
                _authenticationService.GetTenantID(),
                _authenticationService.GetUserID(),
                cancellationToken);
        }

        public async Task<bool> UpdateMyProfileAsync(Employee_MyProfile_Update_DTO model, CancellationToken cancellationToken)
        {
            await _updateMyProfileUseCase.ExecuteAsync(_authenticationService.GetUserID(), new EmployeeProfileUpdate
            {
                UserFirstName = model.UserFirstName,
                UserLastName = model.UserLastName,
                Email = model.Email,
                PhoneNumberPrefix = model.PhoneNumberPrefix,
                PhoneNumber = model.PhoneNumber,
                Language = model.Language,
                Country = model.Country,
                Timezone = model.Timezone
            }, cancellationToken);
            return true;
        }

        private CreateEmployeeCommand MapCreateCommand(Employee_Create_DTO model)
        {
            return new CreateEmployeeCommand
            {
                TenantID = _authenticationService.GetTenantID(),
                CurrentUserID = _authenticationService.GetUserID(),
                SelectedRoleID = model.SelectedRoleID,
                UserAvatar = model.UserAvatar,
                Language = model.Language,
                Country = model.Country,
                Timezone = model.Timezone,
                UserTitle = model.UserTitle,
                UserFirstName = model.UserFirstName,
                UserMiddleName = model.UserMiddleName,
                UserLastName = model.UserLastName,
                Email = model.Email,
                EmailConfirmed = model.EmailConfirmed,
                PhoneNumberPrefix = model.PhoneNumberPrefix,
                PhoneNumber = model.PhoneNumber,
                PhoneNumberConfirmed = model.PhoneNumberConfirmed,
                Gender = model.Gender,
                Pronouns = model.Pronouns,
                DoB = model.DoB,
                PoB = model.PoB,
                SoB = model.SoB,
                CoB = model.CoB,
                Nationality = model.Nationality,
                PrivacyPolicyAcepted = model.PrivacyPolicyAcepted,
                PrivacyPolicyVersion = model.PrivacyPolicyVersion,
                PrivacyPolicyAcceptedDate = model.PrivacyPolicyAcceptedDate,
                LockoutEnd = model.LockoutEnd,
                LockoutEnabled = model.LockoutEnabled,
                AccessFailedCount = model.AccessFailedCount,
                FailedPasswordAnswerAttemptCount = model.FailedPasswordAnswerAttemptCount,
                TimeCost = model.TimeCost,
                BadgeID = model.BadgeID,
                OutOfReports = model.OutOfReports,
                RequireShiftCheckIn = model.RequireShiftCheckIn,
                LastCheckIn = model.LastCheckIn,
                LastCheckOut = model.LastCheckOut,
                Role = model.Role,
                Department = model.Department,
                WorkLocation = model.WorkLocation,
                ContractStartDate = model.ContractStartDate,
                ContractEndDate = model.ContractEndDate,
                MonthlySalary = model.MonthlySalary,
                Bonuses = model.Bonuses,
                Allowances = model.Allowances,
                EmploymentType = model.EmploymentType,
                OvertimeRate = model.OvertimeRate,
                Skills = model.Skills,
                SupervisorID = model.SupervisorID,
                PasswordQuestion = model.TmpPasswordQuestion,
                PasswordAnswer = model.TmpPasswordAnswer,
                AccessLevel = model.AccessLevel,
                AuthorizedAreas = model.AuthorizedAreas,
                InternalNotes = model.InternalNotes,
                PublicNotes = model.PublicNotes,
                ExternalSystemReference = model.ExternalSystemReference,
                SyncStatus = model.SyncStatus,
                NewPassword = model.TmpNewPassword
            };
        }

        private UpdateEmployeeCommand MapUpdateCommand(Guid userID, Employee_Update_DTO model)
        {
            return new UpdateEmployeeCommand
            {
                TenantID = _authenticationService.GetTenantID(),
                CurrentUserID = _authenticationService.GetUserID(),
                UserID = userID,
                UserName = model.UserName,
                UserAvatar = model.UserAvatar,
                Language = model.Language,
                Country = model.Country,
                Timezone = model.Timezone,
                SelectedRoleID = model.SelectedRoleID,
                RoleIDs = model.RoleIDs.ToList(),
                UserTitle = model.UserTitle,
                UserFirstName = model.UserFirstName,
                UserMiddleName = model.UserMiddleName,
                UserLastName = model.UserLastName,
                Email = model.Email,
                EmailConfirmed = model.EmailConfirmed,
                PhoneNumberPrefix = model.PhoneNumberPrefix,
                PhoneNumber = model.PhoneNumber,
                PhoneNumberConfirmed = model.PhoneNumberConfirmed,
                Gender = model.Gender,
                Pronouns = model.Pronouns,
                DoB = model.DoB,
                PoB = model.PoB,
                SoB = model.SoB,
                CoB = model.CoB,
                Nationality = model.Nationality,
                PrivacyPolicyAcepted = model.PrivacyPolicyAcepted,
                PrivacyPolicyVersion = model.PrivacyPolicyVersion,
                PrivacyPolicyAcceptedDate = model.PrivacyPolicyAcceptedDate,
                LockoutEnd = model.LockoutEnd,
                LockoutEnabled = model.LockoutEnabled,
                IsApproved = model.IsApproved,
                AccessFailedCount = model.AccessFailedCount,
                FailedPasswordAnswerAttemptCount = model.FailedPasswordAnswerAttemptCount,
                TimeCost = model.TimeCost,
                BadgeID = model.BadgeID,
                OutOfReports = model.OutOfReports,
                RequireShiftCheckIn = model.RequireShiftCheckIn,
                LastCheckIn = model.LastCheckIn,
                LastCheckOut = model.LastCheckOut,
                Role = model.Role,
                Department = model.Department,
                WorkLocation = model.WorkLocation,
                ContractStartDate = model.ContractStartDate,
                ContractEndDate = model.ContractEndDate,
                MonthlySalary = model.MonthlySalary,
                Bonuses = model.Bonuses,
                Allowances = model.Allowances,
                EmploymentType = model.EmploymentType,
                OvertimeRate = model.OvertimeRate,
                Skills = model.Skills,
                SupervisorID = model.SupervisorID,
                AccessLevel = model.AccessLevel,
                AuthorizedAreas = model.AuthorizedAreas,
                InternalNotes = model.InternalNotes,
                PublicNotes = model.PublicNotes,
                ExternalSystemReference = model.ExternalSystemReference,
                SyncStatus = model.SyncStatus,
                IsDeleted = model.IsDeleted,
                IsDeletedBy = model.IsDeletedBy,
                IsDeletedWhy = model.IsDeletedWhy,
                DateDeleted = model.DateDeleted,
                CreatedBy = model.CreatedBy,
                DateIns = model.DateIns
            };
        }

        private static Employee_List_DTO MapEmployeeList(EmployeeListItem employee)
        {
            return new Employee_List_DTO
            {
                TenantID = employee.TenantID,
                UserID = employee.UserID,
                UserName = employee.UserName,
                UserAvatar = employee.UserAvatar,
                Language = employee.Language,
                Country = employee.Country,
                TimezoneID = employee.TimezoneID,
                Roles = employee.Roles.Select(MapRole).ToList(),
                AssignedPermissions = employee.AssignedPermissions.Select(MapAssignedPermission).ToList(),
                DeniedPermissions = employee.DeniedPermissions.Select(MapDeniedPermission).ToList(),
                UserTitle = employee.UserTitle,
                UserFirstName = employee.UserFirstName,
                UserMiddleName = employee.UserMiddleName,
                UserLastName = employee.UserLastName,
                Email = employee.Email,
                EmailConfirmed = employee.EmailConfirmed,
                PhoneNumberPrefix = employee.PhoneNumberPrefix,
                PhoneNumber = employee.PhoneNumber,
                PhoneNumberConfirmed = employee.PhoneNumberConfirmed,
                Gender = employee.Gender,
                Pronouns = employee.Pronouns,
                PrivacyPolicyAcepted = employee.PrivacyPolicyAcepted,
                PrivacyPolicyVersion = employee.PrivacyPolicyVersion,
                PrivacyPolicyAcceptedDate = employee.PrivacyPolicyAcceptedDate,
                LockoutEnd = employee.LockoutEnd,
                LockoutEnabled = employee.LockoutEnabled,
                IsApproved = employee.IsApproved,
                AccessFailedCount = employee.AccessFailedCount,
                FailedPasswordAnswerAttemptCount = employee.FailedPasswordAnswerAttemptCount,
                TimeCost = employee.TimeCost,
                BadgeID = employee.BadgeID,
                OutOfReports = employee.OutOfReports,
                RequireShiftCheckIn = employee.RequireShiftCheckIn,
                LastCheckIn = employee.LastCheckIn,
                LastCheckOut = employee.LastCheckOut,
                Role = employee.Role,
                Department = employee.Department,
                WorkLocation = employee.WorkLocation,
                AccessLevel = employee.AccessLevel,
                AuthorizedAreas = employee.AuthorizedAreas
            };
        }

        private static Employee_Detail_DTO MapEmployeeDetail(EmployeeDetailResult employee)
        {
            return new Employee_Detail_DTO
            {
                TenantID = employee.TenantID,
                UserID = employee.UserID,
                UserName = employee.UserName,
                UserAvatar = employee.UserAvatar,
                Language = employee.Language,
                Country = employee.Country,
                Timezone = employee.Timezone,
                Roles = employee.Roles.Select(MapRole).ToList(),
                AssignedPermissions = employee.AssignedPermissions.Select(MapAssignedPermission).ToList(),
                DeniedPermissions = employee.DeniedPermissions.Select(MapDeniedPermission).ToList(),
                UserTitle = employee.UserTitle,
                UserFirstName = employee.UserFirstName,
                UserMiddleName = employee.UserMiddleName,
                UserLastName = employee.UserLastName,
                Email = employee.Email,
                EmailConfirmed = employee.EmailConfirmed,
                PhoneNumberPrefix = employee.PhoneNumberPrefix,
                PhoneNumber = employee.PhoneNumber,
                PhoneNumberConfirmed = employee.PhoneNumberConfirmed,
                Gender = employee.Gender,
                Pronouns = employee.Pronouns,
                DoB = employee.DoB,
                PoB = employee.PoB,
                SoB = employee.SoB,
                CoB = employee.CoB,
                Nationality = employee.Nationality,
                PrivacyPolicyAcepted = employee.PrivacyPolicyAcepted,
                PrivacyPolicyVersion = employee.PrivacyPolicyVersion,
                PrivacyPolicyAcceptedDate = employee.PrivacyPolicyAcceptedDate,
                LockoutEnd = employee.LockoutEnd,
                LockoutEnabled = employee.LockoutEnabled,
                IsApproved = employee.IsApproved,
                AccessFailedCount = employee.AccessFailedCount,
                FailedPasswordAnswerAttemptCount = employee.FailedPasswordAnswerAttemptCount,
                TimeCost = employee.TimeCost,
                BadgeID = employee.BadgeID,
                OutOfReports = employee.OutOfReports,
                RequireShiftCheckIn = employee.RequireShiftCheckIn,
                LastCheckIn = employee.LastCheckIn,
                LastCheckOut = employee.LastCheckOut,
                Role = employee.Role,
                Department = employee.Department,
                WorkLocation = employee.WorkLocation,
                ContractStartDate = employee.ContractStartDate,
                ContractEndDate = employee.ContractEndDate,
                MonthlySalary = employee.MonthlySalary,
                Bonuses = employee.Bonuses,
                Allowances = employee.Allowances,
                EmploymentType = employee.EmploymentType,
                OvertimeRate = employee.OvertimeRate,
                Skills = employee.Skills,
                SupervisorID = employee.SupervisorID,
                AccessLevel = employee.AccessLevel,
                AuthorizedAreas = employee.AuthorizedAreas,
                InternalNotes = employee.InternalNotes,
                PublicNotes = employee.PublicNotes,
                ExternalSystemReference = employee.ExternalSystemReference,
                SyncStatus = employee.SyncStatus,
                IsDeleted = employee.IsDeleted,
                IsDeletedBy = employee.IsDeletedBy,
                IsDeletedWhy = employee.IsDeletedWhy,
                DateDeleted = employee.DateDeleted,
                CreatedBy = employee.CreatedBy,
                DateIns = employee.DateIns,
                EditedBy = employee.EditedBy,
                DateEdit = employee.DateEdit
            };
        }

        private static EmployeeRoleDto MapRole(EmployeeRoleResult role)
        {
            return new EmployeeRoleDto
            {
                RoleID = role.RoleID,
                RoleName = role.RoleName,
                NormalizedName = role.NormalizedName,
                ConcurrencyStamp = role.ConcurrencyStamp,
                RolePermissions = role.RolePermissions.Select(p => new EmployeeRolePermissionDto
                {
                    Id = p.Id,
                    RoleId = p.RoleId,
                    Permission = p.Permission
                }).ToList(),
                RoleImage = role.RoleImage,
                IsVisible = role.IsVisible,
                IsDeleted = role.IsDeleted,
                IsDeletedBy = role.IsDeletedBy,
                IsDeletedWhy = role.IsDeletedWhy,
                CreatedBy = role.CreatedBy,
                DateIns = role.DateIns,
                EditedBy = role.EditedBy,
                DateEdit = role.DateEdit
            };
        }

        private static EmployeeUserPermissionDto MapAssignedPermission(EmployeeUserPermissionResult permission)
        {
            return new EmployeeUserPermissionDto
            {
                Id = permission.Id,
                UserId = permission.UserId,
                Permission = permission.Permission
            };
        }

        private static EmployeeUserDeniedPermissionDto MapDeniedPermission(EmployeeUserDeniedPermissionResult permission)
        {
            return new EmployeeUserDeniedPermissionDto
            {
                Id = permission.Id,
                UserId = permission.UserId,
                Permission = permission.Permission
            };
        }

        public async Task<bool> ResendPinAsync(Guid userID, CancellationToken cancellationToken)
        {
            return await _resendEmployeePinUseCase.ExecuteAsync(
                new ResendEmployeePinCommand(_authenticationService.GetTenantID(), userID),
                cancellationToken);
        }

    }
}
