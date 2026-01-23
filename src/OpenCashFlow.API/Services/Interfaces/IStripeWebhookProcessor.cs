using Stripe;

namespace OpenCashFlow.API.Services.Interfaces
{
    public interface IStripeWebhookProcessor
    {
        Task ProcessAsync(Event stripeEvent, CancellationToken cancellationToken);
    }
}
