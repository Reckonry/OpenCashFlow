using global::Shared.Models.Stripe;
using Stripe;

namespace OpenCashFlow.API.Services.Interfaces
{
    public interface IStripeWebhookEventService
    {
        Task<(bool AlreadyProcessed, Stripe_Webhook_Event Record)> RegisterEventAsync(Event stripeEvent, string payload, CancellationToken cancellationToken);
        Task MarkProcessedAsync(Guid recordId, bool success, string? errorMessage, CancellationToken cancellationToken);
    }
}
