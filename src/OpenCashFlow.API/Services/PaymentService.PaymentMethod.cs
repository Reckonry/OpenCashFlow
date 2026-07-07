using OpenCashFlow.Application.Payments.Lookups;
using OpenCashFlow.Application.Payments.PaymentMethods;
using OpenCashFlow.API.Services.Interfaces;
using OpenCashFlow.Contracts.DTOs.Payments;

namespace OpenCashFlow.API.Services
{
    public partial class PaymentService : IPaymentService
    {
        public async Task<IEnumerable<Payment_Method_List_DTO>?> GetAllPaymentMethodsAsync(CancellationToken cancellationToken)
        {
            var methods = await _getPaymentMethodsUseCase.ExecuteAsync(
                new GetPaymentMethodsQuery(_authenticationService.GetTenantID()),
                cancellationToken);

            return methods.Select(ToPaymentMethodListDto).ToList();
        }

        public async Task<Payment_Method_Detail_DTO?> GetPaymentMethodByIdAsync(Guid PaymentMethodID, CancellationToken cancellationToken)
        {
            var method = await _getPaymentMethodDetailUseCase.ExecuteAsync(
                new GetPaymentMethodDetailQuery(PaymentMethodID, _authenticationService.GetTenantID()),
                cancellationToken);

            return method == null ? null : ToPaymentMethodDetailDto(method);
        }

        public async Task<Payment_Method_Create_DTO?> AddPaymentMethodAsync(Payment_Method_Create_DTO paymentMethod, CancellationToken cancellationToken)
        {
            if (paymentMethod == null) return null;

            var command = new PaymentMethodCreateCommand(
                paymentMethod.PaymentMethodID,
                _authenticationService.GetTenantID(),
                _authenticationService.GetUserID(),
                paymentMethod.PaymentMethodName,
                paymentMethod.PaymentMethodDescription,
                paymentMethod.PaymentMethodIcon,
                paymentMethod.Visible,
                paymentMethod.DisplayOrder);

            var created = await _createPaymentMethodUseCase.ExecuteAsync(command, cancellationToken);
            return created == null ? null : ToPaymentMethodCreateDto(created);
        }

        public async Task<Payment_Method_Update_DTO?> UpdatePaymentMethodAsync(Payment_Method_Update_DTO paymentMethod, CancellationToken cancellationToken)
        {
            if (paymentMethod == null) return null;

            var command = new PaymentMethodUpdateCommand(
                paymentMethod.PaymentMethodID,
                _authenticationService.GetTenantID(),
                _authenticationService.GetUserID(),
                paymentMethod.PaymentMethodName,
                paymentMethod.PaymentMethodDescription,
                paymentMethod.PaymentMethodIcon,
                paymentMethod.Visible,
                paymentMethod.DisplayOrder);

            var updated = await _updatePaymentMethodUseCase.ExecuteAsync(command, cancellationToken);
            return updated == null ? null : ToPaymentMethodUpdateDto(updated);
        }

        public async Task<bool> DeletePaymentMethodAsync(Guid PaymentMethodID, CancellationToken cancellationToken)
        {
            return await _deletePaymentMethodUseCase.ExecuteAsync(
                new DeletePaymentMethodCommand(PaymentMethodID, _authenticationService.GetTenantID()),
                cancellationToken);
        }
    }
}
