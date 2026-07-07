using OpenCashFlow.API.Controllers;
using OpenCashFlow.Contracts.DTOs;
using OpenCashFlow.Contracts.DTOs.Payments;

namespace OpenCashFlow.API.Services.Interfaces
{
    public partial interface IPaymentService
    {
        Task<IEnumerable<Payment_List_DTO>?> GetAllPaymentsAsync(Payment_Filter_DTO filters, CancellationToken cancellationToken);
        Task<Payment_Detail_DTO?> GetPaymentByIdAsync(Guid PaymentID, CancellationToken cancellationToken);
        Task<Payment_Create_DTO?> AddPaymentAsync(Payment_Create_DTO payment, CancellationToken cancellationToken);
        Task<Payment_Update_DTO?> UpdatePaymentAsync(Payment_Detail_DTO payment, CancellationToken cancellationToken);
        Task<bool> DeletePaymentAsync(Guid PaymentID, CancellationToken cancellationToken);

        #region PaymentMethod
        Task<IEnumerable<Payment_Method_List_DTO>?> GetAllPaymentMethodsAsync(CancellationToken cancellationToken);
        Task<Payment_Method_Detail_DTO?> GetPaymentMethodByIdAsync(Guid PaymentMethodID, CancellationToken cancellationToken);
        Task<Payment_Method_Create_DTO?> AddPaymentMethodAsync(Payment_Method_Create_DTO paymentMethod, CancellationToken cancellationToken);
        Task<Payment_Method_Update_DTO?> UpdatePaymentMethodAsync(Payment_Method_Update_DTO paymentMethod, CancellationToken cancellationToken);        
        Task<bool> DeletePaymentMethodAsync(Guid PaymentMethodID, CancellationToken cancellationToken);
        #endregion

        #region DocumentType
        Task<IEnumerable<Payment_DocumentType_List_DTO>?> GetAllDocumentTypesAsync(CancellationToken cancellationToken);
        Task<Payment_DocumentType_Detail_DTO?> GetDocumentTypeByIdAsync(Guid DocumentTypeID, CancellationToken cancellationToken);
        Task<Payment_DocumentType_Create_DTO?> AddDocumentTypeAsync(Payment_DocumentType_Create_DTO DocumentType, CancellationToken cancellationToken);
        Task<Payment_DocumentType_Update_DTO?> UpdateDocumentTypeAsync(Payment_DocumentType_Update_DTO DocumentType, CancellationToken cancellationToken);
        Task<bool> DeleteDocumentTypeAsync(Guid DocumentTypeID, CancellationToken cancellationToken);
        #endregion

        #region PaymentReports
        Task<double> GetDailyPaymentsAsync(Guid TenantID, DateTime date, CancellationToken cancellationToken);
        Task<double> GetTotalPaymentsInPeriodAsync(Guid TenantID, DateTime startDate, DateTime endDate, CancellationToken cancellationToken);
        Task<IEnumerable<Payment_DailyPayments>> GetDailyPaymentsInPeriodAsync(Guid TenantID, DateTime startDate, DateTime endDate, CancellationToken cancellationToken);
        Task<double> GetMonthlyPaymentsAsync(Guid TenantID, int year, int month, CancellationToken cancellationToken);
        Task<IEnumerable<Payment_DailyPayments>> GetAllMonthlyPaymentsAsync(Guid TenantID, int year, int month, CancellationToken cancellationToken);
        Task<double> GetYearlyPaymentsAsync(Guid TenantID, int year, CancellationToken cancellationToken);
        Task<IEnumerable<Payment_DailyPayments>> GetAllYearlyPaymentsAsync(Guid TenantID, int year, CancellationToken cancellationToken);
        #endregion

        #region Calendar
        Task<IEnumerable<Payment_CalendarEvent_DTO>> GetPaymentCalendarEventsAsync(Payment_Filter_DTO filters, CancellationToken cancellationToken);
        #endregion
    }
}
