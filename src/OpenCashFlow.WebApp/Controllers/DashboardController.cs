using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenCashFlow.WebApp.Services;
using System.Globalization;
using OpenCashFlow.Contracts.DTOs;

namespace OpenCashFlow.WebApp.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly PaymentAPIService _paymentAPIService;

        public DashboardController(PaymentAPIService paymentAPIService)
        {
            _paymentAPIService = paymentAPIService;
        }

        [Route("")]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var today = DateTime.Today;
            var dailyResp = await _paymentAPIService.GetDailyPaymentsTotalAsync(today, cancellationToken);
            var monthlyResp = await _paymentAPIService.GetMonthlyPaymentsTotalAsync(today.Year, today.Month, cancellationToken);
            var recent = await _paymentAPIService.GetRecentPaymentsAsync(5, cancellationToken);

            // SSR values formatted as currency text for initial render
            if (dailyResp.Success)
                ViewBag.DailyPaymentsTotalText = dailyResp.Data.ToString("C", CultureInfo.CurrentCulture) ?? "—";
            else
                ViewBag.DailyPaymentsTotalText = "—";

            if (monthlyResp.Success)
                ViewBag.MonthlyPaymentsTotalText = monthlyResp.Data.ToString("C", CultureInfo.CurrentCulture) ?? "—";
            else
                ViewBag.MonthlyPaymentsTotalText = "—";

            ViewBag.RecentPayments = recent;

            return View();
        }
    }
}
