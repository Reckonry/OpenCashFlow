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

        // Chiama questo endpoint con: /internal/Payment/EditPaymentModal?paymentID=GUID
        public async Task<IViewComponentResult> InvokeAsync(Guid paymentID)
        {
            ViewBag.EntryTypeList = new List<string> {"Income", "Outcome"};
            ViewBag.PaymentMethodList = await _paymentAPIService.GetPaymentMethodsAsync();
            ViewBag.DocumentTypeList = await _paymentAPIService.GetDocumentTypesAsync();

            // 1) Recupero i dettagli correnti dal servizio API
            var apiResp = await _paymentAPIService.GetPaymentAsync(paymentID);
            if (apiResp == null || !apiResp.Success)
            {
                // Puoi restituire un partial di errore o stringa vuota
                ViewData["Error"] = "Impossibile caricare i dati del pagamento.";
                return View("Error");
            }

            // 2) Mappo Payment_Detail_DTO in Payment_Edit_DTO
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
