using OpenCashFlow.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using global::Shared.Data;
using global::Shared.Models.Stripe;
using Stripe;

namespace OpenCashFlow.API.Services
{
    public class StripeWebhookEventService(ApplicationDbContext dbContext, ILogger<StripeWebhookEventService> logger) : IStripeWebhookEventService
    {
        private readonly ApplicationDbContext _dbContext = dbContext;
        private readonly ILogger<StripeWebhookEventService> _logger = logger;

        public async Task<(bool AlreadyProcessed, Stripe_Webhook_Event Record)> RegisterEventAsync(Event stripeEvent, string payload, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(stripeEvent);

            var eventId = stripeEvent.Id;
            if (string.IsNullOrWhiteSpace(eventId))
            {
                throw new ArgumentException("Stripe event ID is missing", nameof(stripeEvent));
            }

            var existing = await _dbContext.Stripe_Webhook_Events
                .FirstOrDefaultAsync(e => e.EventId == eventId, cancellationToken)
                .ConfigureAwait(false);

            if (existing != null)
            {
                if (existing.Success)
                {
                    _logger.LogInformation("Stripe event {EventId} already processed successfully. Skipping.", eventId);
                    return (true, existing);
                }

                existing.EventType = stripeEvent.Type ?? existing.EventType;
                existing.Payload = payload;
                existing.ReceivedUtc = DateTime.UtcNow;
                existing.ErrorMessage = null;
                await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return (false, existing);
            }

            var createdUtc = stripeEvent.Created != default ? NormalizeToUtc(stripeEvent.Created) : DateTime.UtcNow;

            var record = new Stripe_Webhook_Event
            {
                EventId = eventId,
                EventType = stripeEvent.Type ?? "unknown",
                CreatedUtc = createdUtc,
                ReceivedUtc = DateTime.UtcNow,
                Payload = payload,
                Success = false
            };

            _dbContext.Stripe_Webhook_Events.Add(record);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return (false, record);
        }

        public async Task MarkProcessedAsync(Guid recordId, bool success, string? errorMessage, CancellationToken cancellationToken)
        {
            var record = await _dbContext.Stripe_Webhook_Events
                .FirstOrDefaultAsync(e => e.Id == recordId, cancellationToken)
                .ConfigureAwait(false);

            if (record == null)
            {
                _logger.LogWarning("Stripe webhook event record {RecordId} not found when marking processed.", recordId);
                return;
            }

            record.Success = success;
            record.ErrorMessage = errorMessage;
            record.ProcessedUtc = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        private static DateTime NormalizeToUtc(DateTime dateTime)
        {
            return dateTime.Kind switch
            {
                DateTimeKind.Utc => dateTime,
                DateTimeKind.Local => dateTime.ToUniversalTime(),
                _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
            };
        }
    }
}
