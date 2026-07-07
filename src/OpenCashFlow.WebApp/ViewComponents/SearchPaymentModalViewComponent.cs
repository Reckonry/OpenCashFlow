// /ViewComponents/AddPaymentModalViewComponent.cs
using OpenCashFlow.WebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OpenCashFlow.Contracts.DTOs;

namespace OpenCashFlow.WebApp.ViewComponents
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
