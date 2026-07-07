using Microsoft.Extensions.DependencyInjection;
using OpenCashFlow.Application.Abstractions;
using OpenCashFlow.Application.AuditLog.Ports;
using OpenCashFlow.Application.Auth.Ports;
using OpenCashFlow.Application.Cash.Ports;
using OpenCashFlow.Application.Companies.Ports;
using OpenCashFlow.Application.Employees.Ports;
using OpenCashFlow.Application.Health.Ports;
using OpenCashFlow.Application.Payments.Persistence;
using OpenCashFlow.Application.Payments.Ports;
using OpenCashFlow.Application.Setup.Ports;
using OpenCashFlow.Application.UserManagement.Ports;
using OpenCashFlow.Infrastructure.ApplicationAdapters;
using OpenCashFlow.Infrastructure.Audit;
using OpenCashFlow.Infrastructure.Auth;
using OpenCashFlow.Infrastructure.Cash;
using OpenCashFlow.Infrastructure.Companies;
using OpenCashFlow.Infrastructure.Employees;
using OpenCashFlow.Infrastructure.Health;
using OpenCashFlow.Infrastructure.Payments;
using OpenCashFlow.Infrastructure.Payments.Lookups;
using OpenCashFlow.Infrastructure.Payments.Persistence;
using OpenCashFlow.Infrastructure.Persistence;
using OpenCashFlow.Infrastructure.Roles;
using OpenCashFlow.Infrastructure.Setup;
using OpenCashFlow.Application.Roles.Ports;
using OpenCashFlow.Infrastructure.UserManagement;
using OpenCashFlow.Infrastructure.AuditLog;

namespace OpenCashFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOpenCashFlowInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IPaymentQueryReader, PaymentQueryReader>();
        services.AddScoped<IPaymentReportReader, PaymentReportReader>();
        services.AddScoped<IPaymentCalendarReader, PaymentCalendarReader>();
        services.AddScoped<IPaymentMethodReader, PaymentMethodReader>();
        services.AddScoped<IPaymentMethodWriter, PaymentMethodWriter>();
        services.AddScoped<IDocumentTypeReader, DocumentTypeReader>();
        services.AddScoped<IDocumentTypeWriter, DocumentTypeWriter>();
        services.AddScoped<IPaymentPersistenceReader, PaymentPersistenceReader>();
        services.AddScoped<IPaymentPersistenceWriter, PaymentPersistenceWriter>();
        services.AddScoped<IDailyPaymentPersistence, DailyPaymentPersistence>();
        services.AddScoped<ICashLedgerRepository, CashLedgerRepository>();
        services.AddScoped<ICashReader, CashReader>();
        services.AddScoped<ICashWriter, CashWriter>();
        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<IPaymentReader, PaymentPersistenceReader>();
        services.AddScoped<IPaymentWriter, PaymentPersistenceWriter>();
        services.AddScoped<IDailyPaymentWriter, DailyPaymentPersistence>();
        services.AddScoped<ICashLedgerReader, CashLedgerWriterAdapter>();
        services.AddScoped<ICashLedgerWriter, CashLedgerWriterAdapter>();
        services.AddScoped<IAuditWriter, AuditWriterAdapter>();
        services.AddScoped<ISetupReader, SetupReader>();
        services.AddScoped<ISetupWriter, SetupWriter>();
        services.AddScoped<ICompanyReader, CompanyReader>();
        services.AddScoped<IEmployeeReader, EmployeeReader>();
        services.AddScoped<IEmployeeWriter, EmployeeWriter>();
        services.AddScoped<IEmployeeCredentialService, EmployeeCredentialService>();
        services.AddScoped<IEmployeePinService, EmployeePinService>();
        services.AddScoped<IEmployeeNotificationSender, EmployeeNotificationSender>();
        services.AddScoped<IUserCredentialReader, UserCredentialReader>();
        services.AddScoped<IPasswordResetTokenStore, PasswordResetTokenStore>();
        services.AddScoped<IUserPasswordWriter, UserPasswordWriter>();
        services.AddScoped<IPasswordResetTokenGenerator, PasswordResetTokenGenerator>();
        services.AddScoped<IPasswordResetNotificationSender, PasswordResetNotificationSender>();
        services.AddScoped<IAuthUserReader, AuthUserReader>();
        services.AddScoped<IAuthUserWriter, AuthUserWriter>();
        services.AddScoped<IAuthPasswordVerifier, AuthPasswordVerifier>();
        services.AddScoped<IFastLoginCookieService, FastLoginCookieService>();
        services.AddScoped<IJwtTokenIssuer, JwtTokenIssuer>();
        services.AddScoped<IRegistrationNotificationSender, RegistrationNotificationSender>();
        services.AddScoped<IAuthAuditWriter, AuthAuditWriter>();
        services.AddScoped<IRoleReader, RoleReader>();
        services.AddScoped<IUserManagementStore, UserManagementStore>();
        services.AddScoped<IAuditLogStore, AuditLogStore>();
        services.AddScoped<IDatabaseHealthReader, DatabaseHealthReader>();

        return services;
    }
}
