using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using AutoMapper;
using OpenCashFlow.Contracts.DTOs;
using OpenCashFlow.Contracts.DTOs.Companies;
using OpenCashFlow.Contracts.DTOs.Employees;
using OpenCashFlow.Infrastructure.Persistence.Entities;
using OpenCashFlow.Contracts.DTOs.Payments;
using OpenCashFlow.Infrastructure.Persistence.Entities.Identity;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OpenCashFlow.API.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            #region Payments


            CreateMap<Payment, Payment_List_DTO>()
                .ForMember(dest => dest.PaymentMethodName,
                           opt => opt.MapFrom(src => src.PaymentMethod!.PaymentMethodName))
                .ForMember(dest => dest.DocumentTypeName,
                           opt => opt.MapFrom(src => src.DocumentType!.DocumentTypeName))
                .ForMember(dest => dest.EmployeeFullName,
                           opt => opt.MapFrom(src => src.User!.EmployeeSurnameName))
                .ForMember(dest => dest.DateIns,
                           opt => opt.MapFrom(src => DateTime.SpecifyKind(src.DateIns, DateTimeKind.Utc)));

            CreateMap<Payment, Payment_Update_DTO>()
                .ForMember(dest => dest.PaymentMethodName,
                           opt => opt.MapFrom(src => src.PaymentMethod!.PaymentMethodName))
                .ForMember(dest => dest.DocumentTypeName,
                           opt => opt.MapFrom(src => src.DocumentType!.DocumentTypeName))
                .ForMember(dest => dest.EmployeeFullName,
                           opt => opt.MapFrom(src => src.User!.EmployeeSurnameName))
                .ForMember(dest => dest.DateIns,
                           opt => opt.MapFrom(src => DateTime.SpecifyKind(src.DateIns, DateTimeKind.Utc)));

            CreateMap<Payment, Payment_Detail_DTO>()
                .ForMember(dest => dest.PaymentMethodName,
                           opt => opt.MapFrom(src => src.PaymentMethod!.PaymentMethodName))
                .ForMember(dest => dest.DocumentTypeName,
                           opt => opt.MapFrom(src => src.DocumentType!.DocumentTypeName))
                .ForMember(dest => dest.EmployeeFullName,
                           opt => opt.MapFrom(src => src.User!.EmployeeSurnameName))
                .ForMember(dest => dest.DateIns,
                           opt => opt.MapFrom(src => DateTime.SpecifyKind(src.DateIns, DateTimeKind.Utc)));

            CreateMap<Payment, Payment_Create_DTO>()
                // Explicit mappings when property names differ
                .ForMember(dest => dest.EmployeeFullName,
                           opt => opt.MapFrom(src => src.User!.EmployeeSurnameName));

            CreateMap<Payment_Create_DTO, Payment>();
            CreateMap<Payment_Detail_DTO, Payment>();
            CreateMap<Payment_Update_DTO, Payment>();

            // Payment Methods lookups
            CreateMap<Payment_Method_LookUps, Payment_Method_List_DTO>();
            CreateMap<Payment_Method_LookUps, Payment_Method_Detail_DTO>();
            CreateMap<Payment_Method_LookUps, Payment_Method_Create_DTO>();
            CreateMap<Payment_Method_LookUps, Payment_Method_Update_DTO>();
            CreateMap<Payment_Method_Create_DTO, Payment_Method_LookUps>();
            CreateMap<Payment_Method_Update_DTO, Payment_Method_LookUps>();

            // Payment Document Types lookups
            CreateMap<Payment_DocumentType_LookUp, Payment_DocumentType_List_DTO>();
            CreateMap<Payment_DocumentType_LookUp, Payment_DocumentType_Detail_DTO>();
            CreateMap<Payment_DocumentType_LookUp, Payment_DocumentType_Create_DTO>();
            CreateMap<Payment_DocumentType_LookUp, Payment_DocumentType_Update_DTO>();
            CreateMap<Payment_DocumentType_Create_DTO, Payment_DocumentType_LookUp>();
            CreateMap<Payment_DocumentType_Update_DTO, Payment_DocumentType_LookUp>();

            #endregion

            #region Company
            CreateMap<Company, Company_Detail_DTO>();
            CreateMap<Company_Invoice, Company_Invoices_Detail_DTO>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
            CreateMap<Company_Invoice_Item, Company_Invoice_Item>();
            #endregion

            #region Identity / Roles
            CreateMap<OpenCashFlow.Infrastructure.Persistence.Entities.Identity.AspNetRole, OpenCashFlow.Contracts.DTOs.Identity.Role_List_DTO>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.RoleID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.RoleName));
            #endregion

            #region Employee
            CreateMap<Company_Staff, Employee_List_DTO>()
                .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.User!.UserID))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User!.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User!.Email))
                .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.User!.Language))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.User!.Country))
                .ForMember(dest => dest.TimezoneID, opt => opt.MapFrom(src => src.User!.Timezone))
                .ForMember(dest => dest.UserTitle, opt => opt.MapFrom(src => src.User!.UserTitle))
                .ForMember(dest => dest.UserFirstName, opt => opt.MapFrom(src => src.User!.UserFirstName))
                .ForMember(dest => dest.UserMiddleName, opt => opt.MapFrom(src => src.User!.UserMiddleName))
                .ForMember(dest => dest.UserLastName, opt => opt.MapFrom(src => src.User!.UserLastName))
                .ForMember(dest => dest.UserAvatar, opt => opt.MapFrom(src => src.User!.UserAvatar))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User!.PhoneNumber))
                .ForMember(dest => dest.PhoneNumberPrefix, opt => opt.MapFrom(src => src.User!.PhoneNumberPrefix))
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.MapFrom(src => src.User!.PhoneNumberConfirmed))
                .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => src.User!.EmailConfirmed))
                .ForMember(dest => dest.LockoutEnabled, opt => opt.MapFrom(src => src.User!.LockoutEnabled))
                .ForMember(dest => dest.LockoutEnd, opt => opt.MapFrom(src => src.User!.LockoutEnd))
                .ForMember(dest => dest.IsApproved, opt => opt.MapFrom(src => src.User!.IsApproved))
                .ForMember(dest => dest.AccessFailedCount, opt => opt.MapFrom(src => src.User!.AccessFailedCount))
                .ForMember(dest => dest.FailedPasswordAnswerAttemptCount, opt => opt.MapFrom(src => src.User!.FailedPasswordAnswerAttemptCount))
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.User!.Roles
                    .Where(ur => ur.AspNetRole != null)
                    .Select(ur => ur.AspNetRole!)))
                .ForMember(dest => dest.AssignedPermissions, opt => opt.MapFrom(src => src.User!.AssignedPermissions))
                .ForMember(dest => dest.DeniedPermissions, opt => opt.MapFrom(src => src.User!.DeniedPermissions));

            CreateMap<Company_Staff, Employee_Detail_DTO>()
                .ForMember(dest => dest.TenantID, opt => opt.MapFrom(src => src.TenantID))
                .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.User!.UserID))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User!.UserName))
                .ForMember(dest => dest.UserAvatar, opt => opt.MapFrom(src => src.User!.UserAvatar))
                .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.User!.Language))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.User!.Country))
                .ForMember(dest => dest.Timezone, opt => opt.MapFrom(src => src.User!.Timezone))
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.User!.Roles
                    .Where(ur => ur.AspNetRole != null)
                    .Select(ur => ur.AspNetRole!)))
                .ForMember(dest => dest.AssignedPermissions, opt => opt.MapFrom(src => src.User!.AssignedPermissions))
                .ForMember(dest => dest.DeniedPermissions, opt => opt.MapFrom(src => src.User!.DeniedPermissions))
                .ForMember(dest => dest.UserTitle, opt => opt.MapFrom(src => src.User!.UserTitle))
                .ForMember(dest => dest.UserFirstName, opt => opt.MapFrom(src => src.User!.UserFirstName))
                .ForMember(dest => dest.UserMiddleName, opt => opt.MapFrom(src => src.User!.UserMiddleName))
                .ForMember(dest => dest.UserLastName, opt => opt.MapFrom(src => src.User!.UserLastName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User!.Email))
                .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => src.User!.EmailConfirmed))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User!.PhoneNumber))
                .ForMember(dest => dest.PhoneNumberPrefix, opt => opt.MapFrom(src => src.User!.PhoneNumberPrefix))
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.MapFrom(src => src.User!.PhoneNumberConfirmed))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.User!.Gender))
                .ForMember(dest => dest.Pronouns, opt => opt.MapFrom(src => src.User!.Pronouns))
                .ForMember(dest => dest.DoB, opt => opt.MapFrom(src => src.User!.DoB))
                .ForMember(dest => dest.PoB, opt => opt.MapFrom(src => src.User!.PoB))
                .ForMember(dest => dest.SoB, opt => opt.MapFrom(src => src.User!.SoB))
                .ForMember(dest => dest.CoB, opt => opt.MapFrom(src => src.User!.CoB))
                .ForMember(dest => dest.Nationality, opt => opt.MapFrom(src => src.User!.Nationality))
                .ForMember(dest => dest.PrivacyPolicyAcepted, opt => opt.MapFrom(src => src.User!.PrivacyPolicyAcepted))
                .ForMember(dest => dest.PrivacyPolicyVersion, opt => opt.MapFrom(src => src.User!.PrivacyPolicyVersion))
                .ForMember(dest => dest.PrivacyPolicyAcceptedDate, opt => opt.MapFrom(src => src.User!.PrivacyPolicyAcceptedDate))
                .ForMember(dest => dest.LockoutEnd, opt => opt.MapFrom(src => src.User!.LockoutEnd))
                .ForMember(dest => dest.LockoutEnabled, opt => opt.MapFrom(src => src.User!.LockoutEnabled))
                .ForMember(dest => dest.IsApproved, opt => opt.MapFrom(src => src.User!.IsApproved))
                .ForMember(dest => dest.AccessFailedCount, opt => opt.MapFrom(src => src.User!.AccessFailedCount))
                .ForMember(dest => dest.FailedPasswordAnswerAttemptCount, opt => opt.MapFrom(src => src.User!.FailedPasswordAnswerAttemptCount))
                .ForMember(dest => dest.TimeCost, opt => opt.MapFrom(src => src.TimeCost))
                .ForMember(dest => dest.BadgeID, opt => opt.MapFrom(src => src.BadgeID))
                .ForMember(dest => dest.OutOfReports, opt => opt.MapFrom(src => src.OutOfReports))
                .ForMember(dest => dest.RequireShiftCheckIn, opt => opt.MapFrom(src => src.RequireShiftCheckIn))
                .ForMember(dest => dest.LastCheckIn, opt => opt.MapFrom(src => src.LastCheckIn))
                .ForMember(dest => dest.LastCheckOut, opt => opt.MapFrom(src => src.LastCheckOut))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.Department))
                .ForMember(dest => dest.WorkLocation, opt => opt.MapFrom(src => src.WorkLocation))
                .ForMember(dest => dest.ContractStartDate, opt => opt.MapFrom(src => src.ContractStartDate))
                .ForMember(dest => dest.ContractEndDate, opt => opt.MapFrom(src => src.ContractEndDate))
                .ForMember(dest => dest.MonthlySalary, opt => opt.MapFrom(src => src.MonthlySalary))
                .ForMember(dest => dest.Bonuses, opt => opt.MapFrom(src => src.Bonuses))
                .ForMember(dest => dest.Allowances, opt => opt.MapFrom(src => src.Allowances))
                .ForMember(dest => dest.EmploymentType, opt => opt.MapFrom(src => src.EmploymentType))
                .ForMember(dest => dest.OvertimeRate, opt => opt.MapFrom(src => src.OvertimeRate))
                .ForMember(dest => dest.Skills, opt => opt.MapFrom(src => src.Skills))
                .ForMember(dest => dest.SupervisorID, opt => opt.MapFrom(src => src.SupervisorID))
                .ForMember(dest => dest.AccessLevel, opt => opt.MapFrom(src => src.AccessLevel))
                .ForMember(dest => dest.AuthorizedAreas, opt => opt.MapFrom(src => src.AuthorizedAreas))
                .ForMember(dest => dest.InternalNotes, opt => opt.MapFrom(src => src.InternalNotes))
                .ForMember(dest => dest.PublicNotes, opt => opt.MapFrom(src => src.PublicNotes))
                .ForMember(dest => dest.ExternalSystemReference, opt => opt.MapFrom(src => src.ExternalSystemReference))
                .ForMember(dest => dest.SyncStatus, opt => opt.MapFrom(src => src.SyncStatus))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
                .ForMember(dest => dest.IsDeletedBy, opt => opt.MapFrom(src => src.IsDeletedBy))
                .ForMember(dest => dest.IsDeletedWhy, opt => opt.MapFrom(src => src.IsDeletedWhy))
                .ForMember(dest => dest.DateDeleted, opt => opt.MapFrom(src => src.DateDeleted))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.DateIns, opt => opt.MapFrom(src => src.DateIns))
                .ForMember(dest => dest.EditedBy, opt => opt.MapFrom(src => src.EditedBy))
                .ForMember(dest => dest.DateEdit, opt => opt.MapFrom(src => src.DateEdit));

            #region For creation feature

            CreateMap<Employee_Create_DTO, AspNetUser>()
                .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.UserID))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.UserAvatar, opt => opt.MapFrom(src => src.UserAvatar))
                .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.Language))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
                .ForMember(dest => dest.Timezone, opt => opt.MapFrom(src => src.Timezone))
                .ForMember(dest => dest.UserTitle, opt => opt.MapFrom(src => src.UserTitle))
                .ForMember(dest => dest.UserFirstName, opt => opt.MapFrom(src => src.UserFirstName))
                .ForMember(dest => dest.UserMiddleName, opt => opt.MapFrom(src => src.UserMiddleName))
                .ForMember(dest => dest.UserLastName, opt => opt.MapFrom(src => src.UserLastName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => src.EmailConfirmed))
                .ForMember(dest => dest.PhoneNumberPrefix, opt => opt.MapFrom(src => src.PhoneNumberPrefix))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.MapFrom(src => src.PhoneNumberConfirmed))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.Pronouns, opt => opt.MapFrom(src => src.Pronouns))
                .ForMember(dest => dest.DoB, opt => opt.MapFrom(src => src.DoB))
                .ForMember(dest => dest.PoB, opt => opt.MapFrom(src => src.PoB))
                .ForMember(dest => dest.SoB, opt => opt.MapFrom(src => src.SoB))
                .ForMember(dest => dest.CoB, opt => opt.MapFrom(src => src.CoB))
                .ForMember(dest => dest.Nationality, opt => opt.MapFrom(src => src.Nationality))
                .ForMember(dest => dest.PrivacyPolicyAcepted, opt => opt.MapFrom(src => src.PrivacyPolicyAcepted))
                .ForMember(dest => dest.PrivacyPolicyVersion, opt => opt.MapFrom(src => src.PrivacyPolicyVersion))
                .ForMember(dest => dest.PrivacyPolicyAcceptedDate, opt => opt.MapFrom(src => src.PrivacyPolicyAcepted))
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => "4"))
                .ForMember(dest => dest.PasswordSalt, opt => opt.MapFrom(src => "4"))
                .ForMember(dest => dest.QuickLoginPinHash, opt => opt.MapFrom(src => "4"))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.DateIns, opt => opt.MapFrom(src => src.DateIns))
                .ForMember(dest => dest.Roles, opt => opt.Ignore())
                .ForMember(dest => dest.QuickLoginPinValidUntil, opt => opt.Ignore())
                .ForMember(dest => dest.MobilePin, opt => opt.Ignore())
                .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
                .ForMember(dest => dest.UserMustChangePassword, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordQuestion, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordAnswer, opt => opt.Ignore())
                .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.AccountValidUntil, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordValidUntil, opt => opt.Ignore())
                .ForMember(dest => dest.LastLoginDate, opt => opt.Ignore())
                .ForMember(dest => dest.LastAppLoginDate, opt => opt.Ignore())
                .ForMember(dest => dest.IpAddress, opt => opt.Ignore())
                .ForMember(dest => dest.LastKnownLocation, opt => opt.Ignore())
                .ForMember(dest => dest.AspNetUserClaims, opt => opt.Ignore())
                .ForMember(dest => dest.PrivacyPolicyAcceptedDate, opt => opt.Ignore());

            CreateMap<Employee_Create_DTO, Company_Staff>()
                .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.UserID))
                .ForMember(dest => dest.TenantID, opt => opt.MapFrom(src => src.TenantID))
                .ForMember(dest => dest.TimeCost, opt => opt.MapFrom(src => src.TimeCost))
                .ForMember(dest => dest.BadgeID, opt => opt.MapFrom(src => src.BadgeID))
                .ForMember(dest => dest.OutOfReports, opt => opt.MapFrom(src => src.OutOfReports))
                .ForMember(dest => dest.RequireShiftCheckIn, opt => opt.MapFrom(src => src.RequireShiftCheckIn))
                .ForMember(dest => dest.LastCheckIn, opt => opt.MapFrom(src => src.LastCheckIn))
                .ForMember(dest => dest.LastCheckOut, opt => opt.MapFrom(src => src.LastCheckOut))
                .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.Department))
                .ForMember(dest => dest.WorkLocation, opt => opt.MapFrom(src => src.WorkLocation))
                .ForMember(dest => dest.ContractStartDate, opt => opt.MapFrom(src => src.ContractStartDate))
                .ForMember(dest => dest.ContractEndDate, opt => opt.MapFrom(src => src.ContractEndDate))
                .ForMember(dest => dest.MonthlySalary, opt => opt.MapFrom(src => src.MonthlySalary))
                .ForMember(dest => dest.Bonuses, opt => opt.MapFrom(src => src.Bonuses))
                .ForMember(dest => dest.Allowances, opt => opt.MapFrom(src => src.Allowances))
                .ForMember(dest => dest.EmploymentType, opt => opt.MapFrom(src => src.EmploymentType))
                .ForMember(dest => dest.OvertimeRate, opt => opt.MapFrom(src => src.OvertimeRate))
                .ForMember(dest => dest.Skills, opt => opt.MapFrom(src => src.Skills))
                .ForMember(dest => dest.SupervisorID, opt => opt.MapFrom(src => src.SupervisorID))
                .ForMember(dest => dest.AccessLevel, opt => opt.MapFrom(src => src.AccessLevel))
                .ForMember(dest => dest.AuthorizedAreas, opt => opt.MapFrom(src => src.AuthorizedAreas))
                .ForMember(dest => dest.InternalNotes, opt => opt.MapFrom(src => src.InternalNotes))
                .ForMember(dest => dest.PublicNotes, opt => opt.MapFrom(src => src.PublicNotes))
                .ForMember(dest => dest.ExternalSystemReference, opt => opt.MapFrom(src => src.ExternalSystemReference))
                .ForMember(dest => dest.SyncStatus, opt => opt.MapFrom(src => src.SyncStatus))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.DateIns, opt => opt.MapFrom(src => src.DateIns))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(_ => false));

            // Removed single-role mapping from creation DTO; roles will be managed separately.
            #endregion

            #region For Update feature

            CreateMap<Company_Staff, Employee_Update_DTO>()
                .ForMember(dest => dest.TenantID, opt => opt.MapFrom(src => src.TenantID));
            CreateMap<Employee_Detail_DTO, Employee_Update_DTO>()
                .ForMember(dest => dest.TenantID, opt => opt.Ignore())
                .ForMember(dest => dest.UserID, opt => opt.Ignore())
                .ForMember(dest => dest.UserAvatar, opt => opt.Condition(src => true))
                .ForMember(dest => dest.UserTitle, opt => opt.Condition(src => true))
                .ForMember(dest => dest.UserMiddleName, opt => opt.Condition(src => true))
                .ForMember(dest => dest.PhoneNumberPrefix, opt => opt.Condition(src => true))
                .ForMember(dest => dest.PhoneNumber, opt => opt.Condition(src => true))
                .ForMember(dest => dest.Gender, opt => opt.Condition(src => true))
                .ForMember(dest => dest.Pronouns, opt => opt.Condition(src => true))
                .ForMember(dest => dest.DoB, opt => opt.Condition(src => true))
                .ForMember(dest => dest.PoB, opt => opt.Condition(src => true))
                .ForMember(dest => dest.CoB, opt => opt.Condition(src => true))
                .ForMember(dest => dest.SoB, opt => opt.Condition(src => true))
                .ForMember(dest => dest.Nationality, opt => opt.Condition(src => true))
                .ForMember(dest => dest.LockoutEnd, opt => opt.Condition(src => true))
                .ForMember(dest => dest.AccessFailedCount, opt => opt.Condition(src => true))
                .ForMember(dest => dest.TimeCost, opt => opt.Condition(src => true))
                .ForMember(dest => dest.BadgeID, opt => opt.Condition(src => true))
                .ForMember(dest => dest.Role, opt => opt.Condition(src => true))
                .ForMember(dest => dest.Department, opt => opt.Condition(src => true))
                .ForMember(dest => dest.WorkLocation, opt => opt.Condition(src => true))
                .ForMember(dest => dest.ContractEndDate, opt => opt.Condition(src => true))
                .ForMember(dest => dest.MonthlySalary, opt => opt.Condition(src => true))
                .ForMember(dest => dest.Bonuses, opt => opt.Condition(src => true))
                .ForMember(dest => dest.Allowances, opt => opt.Condition(src => true))
                .ForMember(dest => dest.EmploymentType, opt => opt.Condition(src => true))
                .ForMember(dest => dest.OvertimeRate, opt => opt.Condition(src => true))
                .ForMember(dest => dest.Skills, opt => opt.Condition(src => true))
                .ForMember(dest => dest.SupervisorID, opt => opt.Condition(src => true))
                .ForMember(dest => dest.AccessLevel, opt => opt.Condition(src => true))
                .ForMember(dest => dest.AuthorizedAreas, opt => opt.Condition(src => true))
                .ForMember(dest => dest.InternalNotes, opt => opt.Condition(src => true))
                .ForMember(dest => dest.PublicNotes, opt => opt.Condition(src => true))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Employee_Update_DTO, AspNetUser>()
                .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.UserID))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.UserAvatar, opt => opt.MapFrom(src => src.UserAvatar))
                .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.Language))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
                .ForMember(dest => dest.Timezone, opt => opt.MapFrom(src => src.Timezone))
                .ForMember(dest => dest.UserTitle, opt => opt.MapFrom(src => src.UserTitle))
                .ForMember(dest => dest.UserFirstName, opt => opt.MapFrom(src => src.UserFirstName))
                .ForMember(dest => dest.UserMiddleName, opt => opt.MapFrom(src => src.UserMiddleName))
                .ForMember(dest => dest.UserLastName, opt => opt.MapFrom(src => src.UserLastName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => src.EmailConfirmed))
                .ForMember(dest => dest.PhoneNumberPrefix, opt => opt.MapFrom(src => src.PhoneNumberPrefix))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.MapFrom(src => src.PhoneNumberConfirmed))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.Pronouns, opt => opt.MapFrom(src => src.Pronouns))
                .ForMember(dest => dest.DoB, opt => opt.MapFrom(src => src.DoB))
                .ForMember(dest => dest.PoB, opt => opt.MapFrom(src => src.PoB))
                .ForMember(dest => dest.SoB, opt => opt.MapFrom(src => src.SoB))
                .ForMember(dest => dest.CoB, opt => opt.MapFrom(src => src.CoB))
                .ForMember(dest => dest.Nationality, opt => opt.MapFrom(src => src.Nationality))
                .ForMember(dest => dest.PrivacyPolicyAcepted, opt => opt.MapFrom(src => src.PrivacyPolicyAcepted))
                .ForMember(dest => dest.PrivacyPolicyVersion, opt => opt.MapFrom(src => src.PrivacyPolicyVersion))
                .ForMember(dest => dest.PrivacyPolicyAcceptedDate, opt => opt.MapFrom(src => src.PrivacyPolicyAcepted))
                // Do not modify password-related fields during profile update
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordSalt, opt => opt.Ignore())
                .ForMember(dest => dest.QuickLoginPinHash, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.DateIns, opt => opt.MapFrom(src => src.DateIns))
                .ForMember(dest => dest.Roles, opt => opt.Ignore())
                .ForMember(dest => dest.QuickLoginPinValidUntil, opt => opt.Ignore())
                .ForMember(dest => dest.MobilePin, opt => opt.Ignore())
                .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
                .ForMember(dest => dest.UserMustChangePassword, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordQuestion, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordAnswer, opt => opt.Ignore())
                .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.AccountValidUntil, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordValidUntil, opt => opt.Ignore())
                .ForMember(dest => dest.LastLoginDate, opt => opt.Ignore())
                .ForMember(dest => dest.LastAppLoginDate, opt => opt.Ignore())
                .ForMember(dest => dest.IpAddress, opt => opt.Ignore())
                .ForMember(dest => dest.LastKnownLocation, opt => opt.Ignore())
                .ForMember(dest => dest.AspNetUserClaims, opt => opt.Ignore())
                .ForMember(dest => dest.PrivacyPolicyAcceptedDate, opt => opt.Ignore());

            CreateMap<Employee_Update_DTO, Company_Staff>()
                .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.UserID))
                .ForMember(dest => dest.TenantID, opt => opt.MapFrom(src => src.TenantID))
                .ForMember(dest => dest.TimeCost, opt => opt.MapFrom(src => src.TimeCost))
                .ForMember(dest => dest.BadgeID, opt => opt.MapFrom(src => src.BadgeID))
                .ForMember(dest => dest.OutOfReports, opt => opt.MapFrom(src => src.OutOfReports))
                .ForMember(dest => dest.RequireShiftCheckIn, opt => opt.MapFrom(src => src.RequireShiftCheckIn))
                .ForMember(dest => dest.LastCheckIn, opt => opt.MapFrom(src => src.LastCheckIn))
                .ForMember(dest => dest.LastCheckOut, opt => opt.MapFrom(src => src.LastCheckOut))
                .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.Department))
                .ForMember(dest => dest.WorkLocation, opt => opt.MapFrom(src => src.WorkLocation))
                .ForMember(dest => dest.ContractStartDate, opt => opt.MapFrom(src => src.ContractStartDate))
                .ForMember(dest => dest.ContractEndDate, opt => opt.MapFrom(src => src.ContractEndDate))
                .ForMember(dest => dest.MonthlySalary, opt => opt.MapFrom(src => src.MonthlySalary))
                .ForMember(dest => dest.Bonuses, opt => opt.MapFrom(src => src.Bonuses))
                .ForMember(dest => dest.Allowances, opt => opt.MapFrom(src => src.Allowances))
                .ForMember(dest => dest.EmploymentType, opt => opt.MapFrom(src => src.EmploymentType))
                .ForMember(dest => dest.OvertimeRate, opt => opt.MapFrom(src => src.OvertimeRate))
                .ForMember(dest => dest.Skills, opt => opt.MapFrom(src => src.Skills))
                .ForMember(dest => dest.SupervisorID, opt => opt.MapFrom(src => src.SupervisorID))
                .ForMember(dest => dest.AccessLevel, opt => opt.MapFrom(src => src.AccessLevel))
                .ForMember(dest => dest.AuthorizedAreas, opt => opt.MapFrom(src => src.AuthorizedAreas))
                .ForMember(dest => dest.InternalNotes, opt => opt.MapFrom(src => src.InternalNotes))
                .ForMember(dest => dest.PublicNotes, opt => opt.MapFrom(src => src.PublicNotes))
                .ForMember(dest => dest.ExternalSystemReference, opt => opt.MapFrom(src => src.ExternalSystemReference))
                .ForMember(dest => dest.SyncStatus, opt => opt.MapFrom(src => src.SyncStatus))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.DateIns, opt => opt.MapFrom(src => src.DateIns))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(_ => false));

            // Removed mapping from update DTO to user-role; handled explicitly when syncing RoleIDs.
            #endregion
            #endregion
        }
    }
}
