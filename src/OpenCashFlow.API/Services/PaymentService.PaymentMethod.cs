using AutoMapper;
using OpenCashFlow.API.Repositories.Interfaces;
using OpenCashFlow.API.Services.Interfaces;
using global::Shared.DTOs;
using global::Shared.Models;
using global::Shared.Models.DTOs;

namespace OpenCashFlow.API.Services
{
    public partial class PaymentService : IPaymentService
    {
        public async Task<IEnumerable<Payment_Method_List_DTO>?> GetAllPaymentMethodsAsync(CancellationToken cancellationToken)
        {
            return _mapper.Map<IEnumerable<Payment_Method_List_DTO>>(
                await _paymentRepository.GetAllPaymentMethodsAsync(_authenticationService.GetTenantID(), cancellationToken));
        }

        public async Task<Payment_Method_Detail_DTO?> GetPaymentMethodByIdAsync(Guid PaymentMethodID, CancellationToken cancellationToken)
        {
            return _mapper.Map<Payment_Method_Detail_DTO>(await _paymentRepository.GetPaymentMethodByIdAsync(PaymentMethodID, _authenticationService.GetTenantID(), cancellationToken));
        }

        public async Task<Payment_Method_Create_DTO?> AddPaymentMethodAsync(Payment_Method_Create_DTO paymentMethod, CancellationToken cancellationToken)
        {
            if (paymentMethod == null) return null;
            paymentMethod.TenantID = _authenticationService.GetTenantID();
            paymentMethod.CreatedBy = _authenticationService.GetUserID();
            return _mapper.Map<Payment_Method_Create_DTO>(
                await _paymentRepository.AddPaymentMethodAsync(_mapper.Map<Payment_Method_LookUps>(paymentMethod), cancellationToken));            
        }

        public async Task<Payment_Method_Update_DTO?> UpdatePaymentMethodAsync(Payment_Method_Update_DTO paymentMethod, CancellationToken cancellationToken)
        {
            if (paymentMethod == null || paymentMethod.TenantID == null) return null;
            var paymentMethodTBE = await _paymentRepository.GetPaymentMethodByIdAsync(paymentMethod.PaymentMethodID, _authenticationService.GetTenantID(), cancellationToken);
            if (paymentMethodTBE == null) return null;

            paymentMethodTBE.DisplayOrder = paymentMethod.DisplayOrder;
            paymentMethodTBE.PaymentMethodDescription = paymentMethod.PaymentMethodDescription;
            paymentMethodTBE.PaymentMethodName = paymentMethod.PaymentMethodName;
            paymentMethodTBE.PaymentMethodIcon = paymentMethod.PaymentMethodIcon;
            paymentMethodTBE.Visible = paymentMethod.Visible;
            paymentMethodTBE.DateEdit = DateTime.UtcNow;
            paymentMethodTBE.EditedBy = _authenticationService.GetUserID();
            return _mapper.Map<Payment_Method_Update_DTO>(await _paymentRepository.UpdatePaymentMethodAsync(
                _mapper.Map<Payment_Method_LookUps>(paymentMethodTBE), cancellationToken)); 
        }

        public async Task<bool> DeletePaymentMethodAsync(Guid PaymentMethodID, CancellationToken cancellationToken)
        {
            return await _paymentRepository.DeletePaymentMethodAsync(PaymentMethodID, _authenticationService.GetTenantID(), cancellationToken);
        }
    }
}
