using OpenCashFlow.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using global::Shared.DTOs.Billing;
using global::Shared.Models;
using Stripe;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenCashFlow.API.Controllers
{
    [ApiController, Authorize]
    [Route("v{version:apiVersion}/Billing")]
    [ApiVersion("1.0")]
    public class BillingController(IBillingService billingService, ILogger<BillingController> logger) : Controller
    {
        private readonly IBillingService _billingService = billingService;
        private readonly ILogger<BillingController> _logger = logger;

        [HttpGet("Plans")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BillingPlan_List_DTO>>>> GetPlans([FromQuery] BillingPlan_Filter_DTO filters, CancellationToken cancellationToken)
        {
            filters ??= new BillingPlan_Filter_DTO();
            var plans = await _billingService.GetPlansAsync(filters, cancellationToken);
            return Ok(new ApiResponse<IEnumerable<BillingPlan_List_DTO>>(true, string.Empty, plans));
        }

        [HttpGet("Subscriptions")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BillingSubscription_List_DTO>>>> GetSubscriptions([FromQuery] BillingSubscription_Filter_DTO filters, CancellationToken cancellationToken)
        {
            filters ??= new BillingSubscription_Filter_DTO();
            var subscriptions = await _billingService.GetSubscriptionsAsync(filters, cancellationToken);
            return Ok(new ApiResponse<IEnumerable<BillingSubscription_List_DTO>>(true, string.Empty, subscriptions));
        }

        [HttpGet("Dashboard/KPI")]
        public async Task<ActionResult<ApiResponse<BillingDashboardKPI_DTO>>> GetDashboardKPI(CancellationToken cancellationToken)
        {
            var kpi = await _billingService.GetDashboardKPIAsync(cancellationToken);
            return Ok(new ApiResponse<BillingDashboardKPI_DTO>(true, string.Empty, kpi));
        }

        [HttpPost("CreateCheckout")]
        public async Task<ActionResult<ApiResponse<BillingCheckoutSession_DTO>>> CreateCheckout([FromBody] BillingCheckoutRequest_DTO request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            return await ExecuteAsync(() => _billingService.CreateCheckoutSessionAsync(request, cancellationToken), "Sessione di checkout creata correttamente.");
        }

        [HttpPost("CreatePortalSession")]
        public async Task<ActionResult<ApiResponse<BillingPortalSession_DTO>>> CreatePortalSession([FromBody] BillingPortalSessionRequest_DTO request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            return await ExecuteAsync(() => _billingService.CreateCustomerPortalSessionAsync(request, cancellationToken), "Sessione del customer portal generata.");
        }

        [HttpPost("Subscribe")]
        public async Task<ActionResult<ApiResponse<BillingSubscriptionActionResult_DTO>>> Subscribe([FromBody] BillingSubscribeRequest_DTO request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            return await ExecuteAsync(() => _billingService.CreateSubscriptionAsync(request, cancellationToken), "Subscription creata correttamente.");
        }

        [HttpPut("Subscription/{id:guid}/Cancel")]
        public async Task<ActionResult<ApiResponse<BillingSubscriptionActionResult_DTO>>> CancelSubscription(Guid id, [FromBody] BillingCancelSubscriptionRequest_DTO request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            return await ExecuteAsync(() => _billingService.CancelSubscriptionAsync(id, request.Immediately, cancellationToken), "Subscription cancellata.");
        }

        [HttpPut("Subscription/{id:guid}/Reactivate")]
        public async Task<ActionResult<ApiResponse<BillingSubscriptionActionResult_DTO>>> ReactivateSubscription(Guid id, CancellationToken cancellationToken)
        {
            return await ExecuteAsync(() => _billingService.ReactivateSubscriptionAsync(id, cancellationToken), "Subscription riattivata.");
        }

        [HttpPut("Subscription/{id:guid}/ChangePlan")]
        public async Task<ActionResult<ApiResponse<BillingSubscriptionActionResult_DTO>>> ChangeSubscriptionPlan(Guid id, [FromBody] BillingChangePlanRequest_DTO request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            return await ExecuteAsync(() => _billingService.ChangeSubscriptionPlanAsync(id, request, cancellationToken), "Piano subscription aggiornato.");
        }

        [HttpGet("Subscription/{id:guid}/Invoice")]
        public async Task<ActionResult<ApiResponse<BillingInvoice_DTO>>> GetLatestInvoice(Guid id, CancellationToken cancellationToken)
        {
            return await ExecuteAsync(() => _billingService.GetLatestInvoiceAsync(id, cancellationToken), string.Empty);
        }

        [HttpGet("Subscription/{id:guid}/UpcomingInvoice")]
        public async Task<ActionResult<ApiResponse<BillingInvoice_DTO>>> GetUpcomingInvoice(Guid id, CancellationToken cancellationToken)
        {
            return await ExecuteAsync(() => _billingService.GetUpcomingInvoiceAsync(id, cancellationToken), string.Empty);
        }

        [HttpPost("PaymentMethod")]
        public async Task<ActionResult<ApiResponse<object?>>> AddPaymentMethod([FromBody] BillingAddPaymentMethodRequest_DTO request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            return await ExecuteAsync<object?>(async () =>
            {
                await _billingService.AddPaymentMethodAsync(request, cancellationToken);
                return null;
            }, "Metodo di pagamento aggiunto.");
        }

        [HttpDelete("PaymentMethod/{paymentMethodId}")]
        public async Task<ActionResult<ApiResponse<object?>>> RemovePaymentMethod(string paymentMethodId, CancellationToken cancellationToken)
        {
            return await ExecuteAsync<object?>(async () =>
            {
                await _billingService.RemovePaymentMethodAsync(paymentMethodId, cancellationToken);
                return null;
            }, "Metodo di pagamento rimosso.");
        }

        #region Admin Actions

        [HttpGet("Admin/Subscription/{id:guid}")]
        public async Task<ActionResult<ApiResponse<BillingSubscriptionDetail_DTO>>> GetSubscriptionDetail(Guid id, CancellationToken cancellationToken)
        {
            return await ExecuteAsync(() => _billingService.GetSubscriptionDetailAsync(id, cancellationToken), string.Empty);
        }

        [HttpPost("Admin/Subscription/Sync")]
        public async Task<ActionResult<ApiResponse<BillingAdminActionResult_DTO>>> SyncSubscription([FromBody] BillingSyncSubscriptionRequest_DTO request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            return await ExecuteAsync(() => _billingService.SyncSubscriptionFromStripeAsync(request.SubscriptionID, cancellationToken), "Subscription sincronizzata con successo.");
        }

        [HttpPost("Admin/Subscription/ExtendTrial")]
        public async Task<ActionResult<ApiResponse<BillingAdminActionResult_DTO>>> ExtendTrial([FromBody] BillingExtendTrialRequest_DTO request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            return await ExecuteAsync(() => _billingService.ExtendTrialAsync(request, cancellationToken), $"Trial esteso di {request.Days} giorni.");
        }

        [HttpPost("Admin/Subscription/ApplyCredit")]
        public async Task<ActionResult<ApiResponse<BillingAdminActionResult_DTO>>> ApplyCredit([FromBody] BillingApplyCreditRequest_DTO request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            return await ExecuteAsync(() => _billingService.ApplyCreditAsync(request, cancellationToken), "Credito applicato con successo.");
        }

        [HttpPost("Admin/Invoice/Send")]
        public async Task<ActionResult<ApiResponse<BillingAdminActionResult_DTO>>> SendInvoice([FromBody] BillingSendInvoiceRequest_DTO request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            return await ExecuteAsync(() => _billingService.SendInvoiceManuallyAsync(request, cancellationToken), "Fattura inviata con successo.");
        }

        [HttpPost("Admin/Subscription/AdminCancel")]
        public async Task<ActionResult<ApiResponse<BillingAdminActionResult_DTO>>> AdminCancelSubscription([FromBody] BillingAdminCancelRequest_DTO request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            return await ExecuteAsync(() => _billingService.AdminCancelSubscriptionAsync(request, cancellationToken), "Subscription cancellata.");
        }

        [HttpGet("Admin/Subscription/{id:guid}/WebhookEvents")]
        public async Task<ActionResult<ApiResponse<IEnumerable<StripeWebhookEventSummary_DTO>>>> GetWebhookEvents(Guid id, [FromQuery] int limit = 50, CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(() => _billingService.GetSubscriptionWebhookEventsAsync(id, limit, cancellationToken), string.Empty);
        }

        [HttpPost("Admin/Subscription/ManualUpdate")]
        public async Task<ActionResult<ApiResponse<BillingAdminActionResult_DTO>>> ManualUpdateSubscription([FromBody] BillingManualUpdateSubscriptionRequest_DTO request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            return await ExecuteAsync(() => _billingService.ManualUpdateSubscriptionAsync(request, cancellationToken), "Subscription aggiornata manualmente.");
        }

        [HttpPost("Admin/Subscription/ManualExtend")]
        public async Task<ActionResult<ApiResponse<BillingAdminActionResult_DTO>>> ManualExtendSubscription([FromBody] BillingManualExtendRequest_DTO request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            return await ExecuteAsync(() => _billingService.ManualExtendSubscriptionAsync(request, cancellationToken), "Subscription estesa manualmente.");
        }

        [HttpPost("Admin/Subscription/ManualActivate")]
        public async Task<ActionResult<ApiResponse<BillingAdminActionResult_DTO>>> ManualActivateSubscription([FromBody] BillingManualActivateRequest_DTO request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            return await ExecuteAsync(() => _billingService.ManualActivateSubscriptionAsync(request, cancellationToken), "Subscription attivata/riattivata manualmente.");
        }

        [HttpPost("Admin/Subscription/ChangePlan")]
        public async Task<ActionResult<ApiResponse<BillingAdminActionResult_DTO>>> AdminChangePlan([FromBody] BillingChangePlanRequest_DTO request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            return await ExecuteAsync(() => _billingService.AdminChangePlanAsync(request, cancellationToken), "Piano cambiato con successo.");
        }

        [HttpGet("Admin/Subscription/{id:guid}/AuditLog")]
        public async Task<ActionResult<ApiResponse<List<BillingAuditLogEntry_DTO>>>> GetSubscriptionAuditLog(Guid id, CancellationToken cancellationToken)
        {
            return await ExecuteAsync(() => _billingService.GetSubscriptionAuditLogAsync(id, cancellationToken), "Audit log recuperato con successo.");
        }

        #endregion

        #region Customer Portal

        [HttpGet("Portal")]
        public async Task<ActionResult<ApiResponse<BillingPortal_DTO>>> GetCustomerPortal(CancellationToken cancellationToken)
        {
            return await ExecuteAsync(() => _billingService.GetCustomerBillingPortalAsync(cancellationToken), "Dati billing portal recuperati.");
        }

        #endregion

        private async Task<ActionResult<ApiResponse<T>>> ExecuteAsync<T>(Func<Task<T>> action, string successMessage)
        {
            try
            {
                var result = await action();
                return Ok(new ApiResponse<T>(true, successMessage, result));
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Errore Stripe: {Message}", ex.Message);
                return StatusCode(502, new ApiResponse<T>(false, "Errore nella comunicazione con Stripe", default, new List<string> { ex.Message }));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Risorsa non trovata: {Message}", ex.Message);
                return NotFound(new ApiResponse<T>(false, ex.Message));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Richiesta non valida: {Message}", ex.Message);
                return BadRequest(new ApiResponse<T>(false, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operazione non valida: {Message}", ex.Message);
                return BadRequest(new ApiResponse<T>(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore inatteso");
                return StatusCode(500, new ApiResponse<T>(false, "Si è verificato un errore inatteso", default, new List<string> { ex.Message }));
            }
        }
    }
}
