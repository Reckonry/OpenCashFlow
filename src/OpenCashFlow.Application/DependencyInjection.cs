using Microsoft.Extensions.DependencyInjection;
using OpenCashFlow.Application.AuditLog;
using OpenCashFlow.Application.Auth.AccountConfirmation;
using OpenCashFlow.Application.Auth.FastLogin;
using OpenCashFlow.Application.Auth.ForgotPassword;
using OpenCashFlow.Application.Auth.Login;
using OpenCashFlow.Application.Auth.Register;
using OpenCashFlow.Application.Auth.ResetPassword;
using OpenCashFlow.Application.Cash.CreateCashAdjustment;
using OpenCashFlow.Application.Cash.GetCashBalance;
using OpenCashFlow.Application.Cash.GetCashLedger;
using OpenCashFlow.Application.Cash.RebuildCashBalance;
using OpenCashFlow.Application.Companies.GetCompanies;
using OpenCashFlow.Application.Companies.GetCompany;
using OpenCashFlow.Application.Companies.Invoices;
using OpenCashFlow.Application.Employees.CreateEmployee;
using OpenCashFlow.Application.Employees.DeleteEmployee;
using OpenCashFlow.Application.Employees.GetEmployeeDetail;
using OpenCashFlow.Application.Employees.GetEmployees;
using OpenCashFlow.Application.Employees.ResendPin;
using OpenCashFlow.Application.Employees.UpdateEmployee;
using OpenCashFlow.Application.Employees.UpdateMyProfile;
using OpenCashFlow.Application.Payments.Calendar;
using OpenCashFlow.Application.Payments.CreatePayment;
using OpenCashFlow.Application.Payments.DeletePayment;
using OpenCashFlow.Application.Payments.DocumentTypes;
using OpenCashFlow.Application.Payments.GetPaymentDetail;
using OpenCashFlow.Application.Payments.GetPayments;
using OpenCashFlow.Application.Payments.PaymentMethods;
using OpenCashFlow.Application.Payments.Reports;
using OpenCashFlow.Application.Payments.UpdatePayment;
using OpenCashFlow.Application.Roles.GetRoles;
using OpenCashFlow.Application.Setup.CompleteSetup;
using OpenCashFlow.Application.Setup.GetSetupStatus;
using OpenCashFlow.Application.UserManagement;

namespace OpenCashFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddOpenCashFlowApplication(this IServiceCollection services)
    {
        services.AddScoped<ICreatePaymentUseCase, CreatePaymentUseCase>();
        services.AddScoped<ICreatePaymentOrchestrator, CreatePaymentOrchestrator>();
        services.AddScoped<IUpdatePaymentUseCase, UpdatePaymentUseCase>();
        services.AddScoped<IUpdatePaymentOrchestrator, UpdatePaymentOrchestrator>();
        services.AddScoped<IDeletePaymentUseCase, DeletePaymentUseCase>();
        services.AddScoped<IDeletePaymentOrchestrator, DeletePaymentOrchestrator>();
        services.AddScoped<IGetPaymentsUseCase, GetPaymentsUseCase>();
        services.AddScoped<IGetPaymentDetailUseCase, GetPaymentDetailUseCase>();
        services.AddScoped<IGetPaymentReportsUseCase, GetPaymentReportsUseCase>();
        services.AddScoped<IGetPaymentCalendarUseCase, GetPaymentCalendarUseCase>();
        services.AddScoped<IGetPaymentMethodsUseCase, GetPaymentMethodsUseCase>();
        services.AddScoped<IGetPaymentMethodDetailUseCase, GetPaymentMethodDetailUseCase>();
        services.AddScoped<ICreatePaymentMethodUseCase, CreatePaymentMethodUseCase>();
        services.AddScoped<IUpdatePaymentMethodUseCase, UpdatePaymentMethodUseCase>();
        services.AddScoped<IDeletePaymentMethodUseCase, DeletePaymentMethodUseCase>();
        services.AddScoped<IGetDocumentTypesUseCase, GetDocumentTypesUseCase>();
        services.AddScoped<IGetDocumentTypeDetailUseCase, GetDocumentTypeDetailUseCase>();
        services.AddScoped<ICreateDocumentTypeUseCase, CreateDocumentTypeUseCase>();
        services.AddScoped<IUpdateDocumentTypeUseCase, UpdateDocumentTypeUseCase>();
        services.AddScoped<IDeleteDocumentTypeUseCase, DeleteDocumentTypeUseCase>();
        services.AddScoped<IGetSetupStatusUseCase, GetSetupStatusUseCase>();
        services.AddScoped<ICompleteSetupUseCase, CompleteSetupUseCase>();
        services.AddScoped<IGetCompanyUseCase, GetCompanyUseCase>();
        services.AddScoped<IGetCompaniesUseCase, GetCompaniesUseCase>();
        services.AddScoped<IGetCompanyInvoicesUseCase, GetCompanyInvoicesUseCase>();
        services.AddScoped<IGetCompanyInvoiceDetailUseCase, GetCompanyInvoiceDetailUseCase>();
        services.AddScoped<IGetEmployeesUseCase, GetEmployeesUseCase>();
        services.AddScoped<IGetEmployeeDetailUseCase, GetEmployeeDetailUseCase>();
        services.AddScoped<ICreateEmployeeUseCase, CreateEmployeeUseCase>();
        services.AddScoped<IUpdateEmployeeUseCase, UpdateEmployeeUseCase>();
        services.AddScoped<IDeleteEmployeeUseCase, DeleteEmployeeUseCase>();
        services.AddScoped<IUpdateMyProfileUseCase, UpdateMyProfileUseCase>();
        services.AddScoped<IResendEmployeePinUseCase, ResendEmployeePinUseCase>();
        services.AddScoped<IForgotPasswordUseCase, ForgotPasswordUseCase>();
        services.AddScoped<IValidateResetTokenUseCase, ValidateResetTokenUseCase>();
        services.AddScoped<IResetPasswordUseCase, ResetPasswordUseCase>();
        services.AddScoped<ILoginUseCase, LoginUseCase>();
        services.AddScoped<IFastLoginUseCase, FastLoginUseCase>();
        services.AddScoped<IGenerateFastLoginCookieUseCase, GenerateFastLoginCookieUseCase>();
        services.AddScoped<IRegisterUseCase, RegisterUseCase>();
        services.AddScoped<IConfirmAccountUseCase, ConfirmAccountUseCase>();
        services.AddScoped<IResendConfirmationUseCase, ResendConfirmationUseCase>();
        services.AddScoped<IGetRolesUseCase, GetRolesUseCase>();
        services.AddScoped<IGetCashBalanceUseCase, GetCashBalanceUseCase>();
        services.AddScoped<IGetCashLedgerUseCase, GetCashLedgerUseCase>();
        services.AddScoped<ICreateCashAdjustmentUseCase, CreateCashAdjustmentUseCase>();
        services.AddScoped<IRebuildCashBalanceUseCase, RebuildCashBalanceUseCase>();
        services.AddScoped<IUserManagementUseCase, UserManagementUseCase>();
        services.AddScoped<IAuditLogUseCase, AuditLogUseCase>();

        return services;
    }
}
