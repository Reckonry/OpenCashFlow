using OpenCashFlow.App.Services;
using Microsoft.AspNetCore.Mvc;
using global::Shared.DTOs;

namespace OpenCashFlow.App.ViewComponents
{
    public class EditPaymentModalViewComponent : ViewComponent
    {
        private readonly PaymentAPIService _paymentAPIService;

        public EditPaymentModalViewComponent(PaymentAPIService paymentAPIService)
        {
            _paymentAPIService = paymentAPIService;
        }

        // Call this endpoint with: /internal/Payment/EditPaymentModal?paymentID=GUID
        public async Task<IViewComponentResult> InvokeAsync(Guid paymentID)
        {
            ViewBag.EntryTypeList = new List<string> {"Income", "Outcome"};
            ViewBag.PaymentMethodList = await _paymentAPIService.GetPaymentMethodsAsync();
            ViewBag.DocumentTypeList = await _paymentAPIService.GetDocumentTypesAsync();

            // 1) Retrieve current details from the API service
            var apiResp = await _paymentAPIService.GetPaymentAsync(paymentID);
            if (apiResp == null || !apiResp.Success)
            {
                // You can return an error partial or an empty string
                ViewData["Error"] = "Impossibile caricare i dati del pagamento.";
                return View("Error");
            }

            // 2) Map Payment_Detail_DTO to Payment_Update_DTO
            var detail = apiResp.Data!;
            var editDto = new Payment_Update_DTO
            {
                PaymentID = detail.PaymentID,
                Amount = detail.Amount,
                EntryType = detail.EntryType,
                PaymentMethodID = (Guid)detail.PaymentMethodID,
                PaymentMethodName = detail.PaymentMethodName,
                DocumentTypeID = (Guid)detail.DocumentTypeID,
                DocumentTypeName = detail.DocumentTypeName,
                UserID = detail.UserID,
                EmployeeFullName = detail.EmployeeFullName,
                Description = detail.Description,
                DateIns = detail.DateIns,
            };

            return View(editDto);
        }
    }
}
