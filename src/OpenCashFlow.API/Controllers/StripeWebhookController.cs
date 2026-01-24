using OpenCashFlow.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.Extensions.Options;
using global::Shared.Models.Stripe;
using global::Shared.Options;
using Stripe;

namespace OpenCashFlow.API.Controllers
{
    [ApiController]
    [Route("api/stripe/webhook")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly StripeSettings _stripeSettings;
        private readonly IStripeWebhookEventService _eventService;
        private readonly IStripeWebhookProcessor _processor;
        private readonly ILogger<StripeWebhookController> _logger;

        public StripeWebhookController(
            IOptions<StripeSettings> stripeOptions,
            IStripeWebhookEventService eventService,
            IStripeWebhookProcessor processor,
            ILogger<StripeWebhookController> logger)
        {
            _stripeSettings = stripeOptions.Value;
            _eventService = eventService;
            _processor = processor;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_stripeSettings.WebhookSecret))
            {
                _logger.LogError("Stripe webhook secret is not configured.");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "Stripe webhook secret not configured." });
            }

            Request.EnableBuffering();
            string payload;
            using (var reader = new StreamReader(Request.Body, leaveOpen: true))
            {
                payload = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
                Request.Body.Position = 0;
            }

            if (!Request.Headers.TryGetValue("Stripe-Signature", out var signatureHeaderValues))
            {
                _logger.LogWarning("Stripe webhook request missing signature header.");
                return BadRequest(new { error = "Missing Stripe-Signature header." });
            }

            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(payload, signatureHeaderValues.ToString(), _stripeSettings.WebhookSecret);
            }
            catch (StripeException ex)
            {
                _logger.LogWarning(ex, "Stripe signature validation failed.");
                return BadRequest(new { error = "Invalid signature." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to construct Stripe event.");
                return BadRequest(new { error = "Unable to parse Stripe event." });
            }

            Stripe_Webhook_Event? eventRecord = null;
            try
            {
                var registration = await _eventService.RegisterEventAsync(stripeEvent, payload, cancellationToken).ConfigureAwait(false);
                eventRecord = registration.Record;

                if (registration.AlreadyProcessed)
                {
                    return Ok(new { status = "duplicate" });
                }

                await _processor.ProcessAsync(stripeEvent, cancellationToken).ConfigureAwait(false);
                await _eventService.MarkProcessedAsync(eventRecord.Id, success: true, errorMessage: null, cancellationToken).ConfigureAwait(false);

                return Ok(new { status = "processed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Stripe event {EventId}", stripeEvent.Id);
                try
                {
                    if (eventRecord != null)
                    {
                        await _eventService.MarkProcessedAsync(eventRecord.Id, success: false, errorMessage: ex.Message, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (Exception innerEx)
                {
                    _logger.LogWarning(innerEx, "Failed to update webhook event status for event {EventId}", stripeEvent.Id);
                }

                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to process event." });
            }
        }
    }
}
