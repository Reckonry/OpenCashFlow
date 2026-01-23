// /ViewComponents/AddPaymentModalViewComponent.cs
using OpenCashFlow.App.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using global::Shared.DTOs;

namespace OpenCashFlow.App.ViewComponents
{
    public class SearchPaymentsModalViewComponent : ViewComponent
    {
        private readonly PaymentAPIService _paymentAPIService;

        public SearchPaymentsModalViewComponent(PaymentAPIService paymentAPIService)
        {
            _paymentAPIService = paymentAPIService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new Payment_Filter_DTO();

            ViewBag.DocumentTypeList = await _paymentAPIService.GetDocumentTypesAsync();
            ViewBag.PaymentMethodList = await _paymentAPIService.GetPaymentMethodsAsync();

            return View(model);
        }
    }
}
