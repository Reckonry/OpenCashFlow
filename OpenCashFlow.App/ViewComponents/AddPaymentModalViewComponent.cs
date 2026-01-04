// /ViewComponents/AddPaymentModalViewComponent.cs
using OpenCashFlow.App.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using global::Shared.DTOs;

namespace OpenCashFlow.App.ViewComponents
{
    public class AddPaymentModalViewComponent : ViewComponent
    {
        private readonly ILogger<AddPaymentModalViewComponent> _logger;
        private readonly PaymentAPIService _paymentAPIService;

        public AddPaymentModalViewComponent(ILogger<AddPaymentModalViewComponent> logger, PaymentAPIService PaymentAPIService)
        {
            _logger = logger;
            _paymentAPIService = PaymentAPIService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            ViewBag.EntryTypeList = new List<string> {"Income", "Outcome"};
            ViewBag.PaymentMethodList = await _paymentAPIService.GetPaymentMethodsAsync();
            ViewBag.DocumentTypeList = await _paymentAPIService.GetDocumentTypesAsync();
            //var model = new Payment_Create_DTO
            //{
            //    EntryTypeList = (await _lookup.GetEntryTypesAsync())
            //                            .Select(x => new SelectListItem(x.Name, x.Value)),
            //    PaymentMethodList = (await _lookup.GetPaymentMethodsAsync())
            //                            .Select(x => new SelectListItem(x.Name, x.Id.ToString())),
            //    DocumentTypeList = (await _lookup.GetDocumentTypesAsync())
            //                            .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
            //};
            return View(); // chiama Views/Shared/Components/AddPaymentModal/Default.cshtml
        }
    }
}
