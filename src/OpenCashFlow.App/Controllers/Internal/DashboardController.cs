using OpenCashFlow.App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using global::Shared.DTOs;

namespace OpenCashFlow.App.Controllers.Internal
{
    [Authorize]
    [Route("internal/[controller]/[action]")]
    public class DashboardController : Controller
    {
        private readonly PaymentAPIService _paymentAPIService;

        public DashboardController(PaymentAPIService paymentAPIService)
        {
            _paymentAPIService = paymentAPIService;
        }

        [HttpGet]
        public async Task<IActionResult> DailyTotal(DateTime? date)
        {
            var d = date?.Date ?? DateTime.Today;
            var resp = await _paymentAPIService.GetDailyPaymentsTotalAsync(d);
            var amountText = resp.Success
                ? (resp.Data.ToString("C", new CultureInfo("it-IT")) ?? "—")
                : "—";
            ViewData["AmountText"] = amountText;
            return PartialView("/Views/Dashboard/Components/Fragments/_DailyTotalValue.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> MonthlyTotal(int? year, int? month)
        {
            var today = DateTime.Today;
            var y = year ?? today.Year;
            var m = month ?? today.Month;
            var resp = await _paymentAPIService.GetMonthlyPaymentsTotalAsync(y, m);
            var amountText = resp.Success
                ? (resp.Data.ToString("C", new CultureInfo("it-IT")) ?? "—")
                : "—";
            ViewData["AmountText"] = amountText;
            return PartialView("/Views/Dashboard/Components/Fragments/_MonthlyTotalValue.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> RecentPayments(int count = 5)
        {
            var items = await _paymentAPIService.GetRecentPaymentsAsync(count);
            return PartialView("/Views/Dashboard/Components/Fragments/_RecentPaymentsTimeline.cshtml", items);
        }
    }
}
