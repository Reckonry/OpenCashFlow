using Microsoft.Extensions.DependencyInjection;
using OpenCashFlow.Application.Payments.Calendar;
using OpenCashFlow.Application.Payments.CreatePayment;
using OpenCashFlow.Application.Payments.DeletePayment;
using OpenCashFlow.Application.Payments.DocumentTypes;
using OpenCashFlow.Application.Payments.GetPaymentDetail;
using OpenCashFlow.Application.Payments.GetPayments;
using OpenCashFlow.Application.Payments.PaymentMethods;
using OpenCashFlow.Application.Payments.Reports;
using OpenCashFlow.Application.Payments.UpdatePayment;

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

        return services;
    }
}
