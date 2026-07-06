using OpenCashFlow.WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using global::Shared.DTOs;

namespace OpenCashFlow.WebApp.Controllers
{
    [Authorize]
    [Route("[Controller]")]
    public class PaymentsController : Controller
    {
        private readonly ILogger<PaymentsController> _logger;
        private readonly PaymentAPIService _paymentAPIService;

        public PaymentsController(ILogger<PaymentsController> logger, PaymentAPIService PaymentAPIService)
        {
            _logger = logger;
            _paymentAPIService = PaymentAPIService;
        }

        [HttpGet(""), ActionName("Index")]
        public async Task<IActionResult> IndexAsync()
        {
            Payment_Filter_DTO filters = new();
            IEnumerable<Payment_List_DTO>? x = await _paymentAPIService.GetPaymentsAsync(filters);
            return View(x);
        }

        [HttpGet("Calendar"), ActionName("Calendar")]
        public IActionResult Calendar()
        {
            return View();
        }

    }
}
