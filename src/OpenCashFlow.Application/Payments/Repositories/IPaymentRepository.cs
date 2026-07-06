using global::Shared.Models;

namespace OpenCashFlow.Application.Payments.Repositories
{
    public partial interface IPaymentRepository
    {
        Task<Payment?> GetPaymentByIdAsync(Guid PaymentID, Guid TenantID, CancellationToken cancellationToken);
        Task<Payment?> GetPaymentByIdForUpdateAsync(Guid PaymentID, Guid TenantID, CancellationToken cancellationToken);
        Task<Payment?> GetPaymentByRequestIdAsync(Guid requestId, Guid TenantID, CancellationToken cancellationToken);
        Task<Payment?> AddPaymentAsync(Payment payment, CancellationToken cancellationToken);
        Task<Payment?> UpdatePaymentAsync(Payment payment, CancellationToken cancellationToken);
        Task<bool> DeletePaymentAsync(Guid PaymentID, Guid TenantID, CancellationToken cancellationToken);

        #region Payment Reports
        Task<Payment_DailyPayments> UpdateDailyPaymentAsync(Guid TenantID, DateTime date, double amount, string entryType, CancellationToken cancellationToken);
        Task<bool> DeleteDailyPaymentAsync(Guid TenantID, DateTime date, double amount, string entryType, CancellationToken cancellationToken);
        #endregion
    }
}
