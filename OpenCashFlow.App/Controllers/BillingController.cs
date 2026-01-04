using OpenCashFlow.App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OpenCashFlow.App.Controllers
{
    [Authorize]
    public class BillingController(BillingAPIService billingService, ILogger<BillingController> logger) : Controller
    {
        private readonly BillingAPIService _billingService = billingService;
        private readonly ILogger<BillingController> _logger = logger;

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var response = await _billingService.GetCustomerPortalAsync(cancellationToken);

            if (!response.Success || response.Data == null)
            {
                _logger.LogWarning("Failed to load billing portal: {Message}", response.Message);
                ViewData["ErrorMessage"] = response.Message ?? "Errore durante il caricamento dei dati di fatturazione.";
                return View(new global::Shared.DTOs.Billing.BillingPortal_DTO());
            }

            ViewData["SuccessMessage"] = TempData["SuccessMessage"] as string;
            return View(response.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OpenCustomerPortal(string returnUrl, CancellationToken cancellationToken)
        {
            returnUrl ??= Url.Action("Index", "Billing") ?? "/Billing";

            var response = await _billingService.CreateCustomerPortalSessionAsync(returnUrl, cancellationToken);

            if (!response.Success || response.Data == null)
            {
                _logger.LogError("Failed to create customer portal session: {Message}", response.Message);
                TempData["ErrorMessage"] = response.Message ?? "Errore durante l'apertura del portale pagamenti.";
                return RedirectToAction("Index");
            }

            // Redirect to Stripe Customer Portal
            return Redirect(response.Data.Url);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePlan(Guid subscriptionId, Guid newPlanId, CancellationToken cancellationToken)
        {
            if (subscriptionId == Guid.Empty || newPlanId == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Parametri non validi.";
                return RedirectToAction("Index");
            }

            var response = await _billingService.ChangePlanAsync(subscriptionId, newPlanId, cancellationToken);

            if (!response.Success)
            {
                _logger.LogError("Failed to change plan: {Message}", response.Message);
                TempData["ErrorMessage"] = response.Message ?? "Errore durante il cambio piano.";
            }
            else
            {
                TempData["SuccessMessage"] = response.Message ?? "Piano cambiato con successo!";
            }

            return RedirectToAction("Index");
        }
    }
}
