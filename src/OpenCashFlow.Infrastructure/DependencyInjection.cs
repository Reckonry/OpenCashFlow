using Microsoft.Extensions.DependencyInjection;
using OpenCashFlow.Application.Abstractions;
using OpenCashFlow.Application.Payments.Ports;
using OpenCashFlow.Application.Payments.Repositories;
using OpenCashFlow.Infrastructure.ApplicationAdapters;
using OpenCashFlow.Infrastructure.Audit;
using OpenCashFlow.Infrastructure.Cash;
using OpenCashFlow.Infrastructure.Payments;
using OpenCashFlow.Infrastructure.Payments.Lookups;
using OpenCashFlow.Infrastructure.Persistence;

namespace OpenCashFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOpenCashFlowInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPaymentQueryReader, PaymentQueryReader>();
        services.AddScoped<IPaymentReportReader, PaymentReportReader>();
        services.AddScoped<IPaymentCalendarReader, PaymentCalendarReader>();
        services.AddScoped<IPaymentMethodReader, PaymentMethodReader>();
        services.AddScoped<IPaymentMethodWriter, PaymentMethodWriter>();
        services.AddScoped<IDocumentTypeReader, DocumentTypeReader>();
        services.AddScoped<IDocumentTypeWriter, DocumentTypeWriter>();
        services.AddScoped<ICashLedgerRepository, CashLedgerRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<IPaymentReader, PaymentReaderAdapter>();
        services.AddScoped<IPaymentWriter, PaymentWriterAdapter>();
        services.AddScoped<IDailyPaymentWriter, DailyPaymentWriterAdapter>();
        services.AddScoped<ICashLedgerReader, CashLedgerWriterAdapter>();
        services.AddScoped<ICashLedgerWriter, CashLedgerWriterAdapter>();
        services.AddScoped<IAuditWriter, AuditWriterAdapter>();

        return services;
    }
}
