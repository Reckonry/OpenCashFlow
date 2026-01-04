using Microsoft.EntityFrameworkCore;
using global::Shared.DTOs;
using global::Shared.Models;
using System.Threading.Tasks;

namespace OpenCashFlow.API.Repositories.Interfaces
{
    public partial interface IPaymentRepository
    {
        Task<IEnumerable<Payment>?> GetAllPaymentsAsync(Guid TenantID, Payment_Filter_DTO filters, CancellationToken cancellationToken);
        Task<Payment?> GetPaymentByIdAsync(Guid PaymentID, Guid TenantID, CancellationToken cancellationToken);
        Task<Payment?> GetPaymentByIdForUpdateAsync(Guid PaymentID, Guid TenantID, CancellationToken cancellationToken);
        Task<Payment?> GetPaymentByRequestIdAsync(Guid requestId, Guid TenantID, CancellationToken cancellationToken);
        Task<Payment?> AddPaymentAsync(Payment payment, CancellationToken cancellationToken);
        Task<Payment?> UpdatePaymentAsync(Payment payment, CancellationToken cancellationToken);
        Task<bool> DeletePaymentAsync(Guid PaymentID, Guid TenantID, CancellationToken cancellationToken);

        #region PaymentMethod
        Task<IEnumerable<Payment_Method_LookUps>?> GetAllPaymentMethodsAsync(Guid TenantID, CancellationToken cancellationToken);
        Task<Payment_Method_LookUps?> GetPaymentMethodByIdAsync(Guid PaymentMethodID, Guid TenantID, CancellationToken cancellationToken);
        Task<Payment_Method_LookUps?> AddPaymentMethodAsync(Payment_Method_LookUps paymentMethod, CancellationToken cancellationToken);
        Task<Payment_Method_LookUps?> UpdatePaymentMethodAsync(Payment_Method_LookUps paymentMethod, CancellationToken cancellationToken);
        Task<bool> DeletePaymentMethodAsync(Guid PaymentMethodID, Guid TenantID, CancellationToken cancellationToken);
        #endregion

        #region DocumentType
        Task<IEnumerable<Payment_DocumentType_LookUp>?> GetAllDocumentTypesAsync(Guid TenantID, CancellationToken cancellationToken);
        Task<Payment_DocumentType_LookUp?> GetDocumentTypeByIdAsync(Guid DocumentTypeID, Guid TenantID, CancellationToken cancellationToken);
        Task<Payment_DocumentType_LookUp?> AddPaymentDocumentTypeAsync(Payment_DocumentType_LookUp DocumentType, CancellationToken cancellationToken);
        Task<Payment_DocumentType_LookUp?> UpdateDocumentTypeAsync(Payment_DocumentType_LookUp DocumentType, CancellationToken cancellationToken);
        Task<bool> DeleteDocumentTypeAsync(Guid DocumentTypeID, Guid TenantID, CancellationToken cancellationToken);
        #endregion

        #region Payment Reports
        Task<double> GetDailyPaymentAsync(Guid TenantID, DateTime date, CancellationToken cancellationToken);
        Task<IEnumerable<Payment_DailyPayments>> GetAllDailyPaymentsAsync(Guid TenantID, CancellationToken cancellationToken);
        Task<Payment_DailyPayments> UpdateDailyPaymentAsync(Guid TenantID, DateTime date, double amount, string entryType, CancellationToken cancellationToken);
        Task<bool> DeleteDailyPaymentAsync(Guid TenantID, DateTime date, double amount, string entryType, CancellationToken cancellationToken);
        Task<double> GetTotalPaymentsInPeriodAsync(Guid TenantID, DateTime startDate, DateTime endDate, CancellationToken cancellationToken);
        Task<IEnumerable<Payment_DailyPayments>> GetDailyPaymentsInPeriodAsync(Guid TenantID, DateTime startDate, DateTime endDate, CancellationToken cancellationToken);
        Task<double> GetMonthlyPaymentsAsync(Guid TenantID, int year, int month, CancellationToken cancellationToken);
        Task<IEnumerable<Payment_DailyPayments>> GetAllMonthlyPaymentsAsync(Guid TenantID, int year, int month, CancellationToken cancellationToken);
        Task<double> GetYearlyPaymentsAsync(Guid TenantID, int year, CancellationToken cancellationToken);
        Task<IEnumerable<Payment_DailyPayments>> GetAllYearlyPaymentsAsync(Guid TenantID, int year, CancellationToken cancellationToken);
        #endregion

        #region Calendar
        Task<IEnumerable<Payment_CalendarEvent_DTO>> GetPaymentCalendarEventsAsync(Guid TenantID, Payment_Filter_DTO filters, CancellationToken cancellationToken);
        #endregion
    }
}
