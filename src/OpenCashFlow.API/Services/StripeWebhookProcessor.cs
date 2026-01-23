using OpenCashFlow.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using global::Shared.Data;
using Stripe;
using CompanySubscriptionModel = global::Shared.Models.Company_Subscription;
using BillingCycle = global::Shared.Models.BillingCycle;
using RenewalStatus = global::Shared.Models.RenewalStatus;
using System.Linq;

namespace OpenCashFlow.API.Services
{
    public class StripeWebhookProcessor(
        IStripeSyncService stripeSyncService,
        ApplicationDbContext dbContext,
        ILogger<StripeWebhookProcessor> logger) : IStripeWebhookProcessor
    {
        private readonly IStripeSyncService _stripeSyncService = stripeSyncService;
        private readonly ApplicationDbContext _dbContext = dbContext;
        private readonly ILogger<StripeWebhookProcessor> _logger = logger;

        public async Task ProcessAsync(Event stripeEvent, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(stripeEvent);

            _logger.LogInformation("Processing Stripe event {EventId} of type {EventType}", stripeEvent.Id, stripeEvent.Type);

            switch (stripeEvent.Type)
            {
                case EventTypes.CustomerSubscriptionCreated:
                case EventTypes.CustomerSubscriptionUpdated:
                case EventTypes.CustomerSubscriptionDeleted:
                case EventTypes.CustomerSubscriptionPaused:
                case EventTypes.CustomerSubscriptionResumed:
                case EventTypes.CustomerSubscriptionPendingUpdateApplied:
                case EventTypes.CustomerSubscriptionPendingUpdateExpired:
                    await HandleSubscriptionEventAsync(stripeEvent, cancellationToken).ConfigureAwait(false);
                    break;

                case EventTypes.CustomerSubscriptionTrialWillEnd:
                    await HandleTrialWillEndAsync(stripeEvent, cancellationToken).ConfigureAwait(false);
                    break;

                case EventTypes.CustomerUpdated:
                    await HandleCustomerUpdatedAsync(stripeEvent, cancellationToken).ConfigureAwait(false);
                    break;

                case EventTypes.CustomerDeleted:
                    await HandleCustomerDeletedAsync(stripeEvent, cancellationToken).ConfigureAwait(false);
                    break;

                case EventTypes.PaymentIntentSucceeded:
                    await HandlePaymentIntentAsync(stripeEvent, success: true, cancellationToken).ConfigureAwait(false);
                    break;
                case EventTypes.PaymentIntentPaymentFailed:
                    await HandlePaymentIntentAsync(stripeEvent, success: false, cancellationToken).ConfigureAwait(false);
                    break;
                case EventTypes.PaymentMethodAttached:
                    await HandlePaymentMethodEventAsync(stripeEvent, attached: true, cancellationToken).ConfigureAwait(false);
                    break;
                case EventTypes.PaymentMethodDetached:
                    await HandlePaymentMethodEventAsync(stripeEvent, attached: false, cancellationToken).ConfigureAwait(false);
                    break;
                case EventTypes.InvoiceCreated:
                    await HandleInvoiceEventAsync(stripeEvent, InvoiceEventKind.Created, cancellationToken).ConfigureAwait(false);
                    break;
                case EventTypes.InvoiceFinalized:
                    await HandleInvoiceEventAsync(stripeEvent, InvoiceEventKind.Finalized, cancellationToken).ConfigureAwait(false);
                    break;
                case EventTypes.InvoicePaid:
                    await HandleInvoiceEventAsync(stripeEvent, InvoiceEventKind.Paid, cancellationToken).ConfigureAwait(false);
                    break;
                case EventTypes.InvoicePaymentFailed:
                    await HandleInvoiceEventAsync(stripeEvent, InvoiceEventKind.PaymentFailed, cancellationToken).ConfigureAwait(false);
                    break;
                case EventTypes.InvoicePaymentActionRequired:
                    await HandleInvoiceEventAsync(stripeEvent, InvoiceEventKind.PaymentActionRequired, cancellationToken).ConfigureAwait(false);
                    break;

                default:
                    _logger.LogInformation("Received unhandled Stripe event type {EventType}.", stripeEvent.Type);
                    break;
            }
        }

        private async Task HandleSubscriptionEventAsync(Event stripeEvent, CancellationToken cancellationToken)
        {
            var subscription = DeserializeEntity<Subscription>(stripeEvent);
            if (!string.IsNullOrWhiteSpace(subscription?.Id))
            {
                await _stripeSyncService.SyncSubscriptionFromStripeAsync(subscription.Id, cancellationToken).ConfigureAwait(false);
            }
            else if (!string.IsNullOrWhiteSpace(subscription?.CustomerId))
            {
                _logger.LogInformation("Subscription payload missing ID for event {EventId}. Forcing customer sync.", stripeEvent.Id);
                await _stripeSyncService.SyncCustomerFromStripeAsync(subscription.CustomerId, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                _logger.LogWarning("Unable to process subscription event {EventId}: missing subscription information.", stripeEvent.Id);
            }
        }

        private async Task HandleTrialWillEndAsync(Event stripeEvent, CancellationToken cancellationToken)
        {
            var subscription = DeserializeEntity<Subscription>(stripeEvent);
            _logger.LogInformation("Subscription {SubscriptionId} trial will end on {TrialEnd} (event {EventId}).",
                subscription?.Id,
                subscription?.TrialEnd,
                stripeEvent.Id);
            if (subscription?.Id == null)
            {
                return;
            }

            var local = await _stripeSyncService.SyncSubscriptionFromStripeAsync(subscription.Id, cancellationToken).ConfigureAwait(false);
            if (local != null && subscription.TrialEnd.HasValue)
            {
                local.NextReminderDate = NormalizeToUtc(subscription.TrialEnd.Value);
                local.DateEdit = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task HandleCustomerUpdatedAsync(Event stripeEvent, CancellationToken cancellationToken)
        {
            var customer = DeserializeEntity<Customer>(stripeEvent);
            if (!string.IsNullOrWhiteSpace(customer?.Id))
            {
                await _stripeSyncService.SyncCustomerFromStripeAsync(customer.Id, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                _logger.LogWarning("Unable to process customer.updated event {EventId}: missing customer ID.", stripeEvent.Id);
            }
        }

        private async Task HandleCustomerDeletedAsync(Event stripeEvent, CancellationToken cancellationToken)
        {
            var customer = DeserializeEntity<Customer>(stripeEvent);
            if (string.IsNullOrWhiteSpace(customer?.Id))
            {
                _logger.LogWarning("Unable to process customer.deleted event {EventId}: missing customer ID.", stripeEvent.Id);
                return;
            }

            var company = await _dbContext.Company_DS
                .FirstOrDefaultAsync(c => c.StripeCustomerID == customer.Id, cancellationToken)
                .ConfigureAwait(false);

            if (company == null)
            {
                _logger.LogInformation("Customer {CustomerId} deleted in Stripe but no local company was linked.", customer.Id);
                return;
            }

            company.StripeCustomerID = null;
            company.StripeDefaultPaymentMethodID = null;
            company.DateEdit = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Cleared Stripe references for company {CompanyId} due to customer deletion.", company.TenantID);
        }

        private async Task HandlePaymentIntentAsync(Event stripeEvent, bool success, CancellationToken cancellationToken)
        {
            var intent = DeserializeEntity<PaymentIntent>(stripeEvent);
            if (intent == null)
            {
                _logger.LogWarning("PaymentIntent event {EventId} missing payload.", stripeEvent.Id);
                return;
            }

            if (!string.IsNullOrWhiteSpace(intent.CustomerId))
            {
                await _stripeSyncService.SyncCustomerFromStripeAsync(intent.CustomerId, cancellationToken).ConfigureAwait(false);
            }

            var raw = stripeEvent.Data.RawJObject;
            var subscriptionId = raw?["subscription"]?.Value<string>();
            var invoiceId = raw?["invoice"]?.Value<string>() ?? intent.Metadata?.GetValueOrDefault("invoice_id");

            CompanySubscriptionModel? subscription = null;
            if (!string.IsNullOrWhiteSpace(subscriptionId))
            {
                subscription = await _stripeSyncService.SyncSubscriptionFromStripeAsync(subscriptionId, cancellationToken).ConfigureAwait(false);
            }

            subscription ??= await FindSubscriptionAsync(subscriptionId, invoiceId, intent.CustomerId, cancellationToken).ConfigureAwait(false);

            if (subscription == null)
            {
                _logger.LogInformation("PaymentIntent {PaymentIntentId} processed but no local subscription was matched.", intent.Id);
                return;
            }

            var modified = false;
            if (!string.IsNullOrWhiteSpace(invoiceId) && !string.Equals(subscription.StripeInvoiceID, invoiceId, StringComparison.Ordinal))
            {
                subscription.StripeInvoiceID = invoiceId;
                modified = true;
            }

            if (success)
            {
                if (subscription.RenewalStatus != RenewalStatus.ACTIVE)
                {
                    subscription.RenewalStatus = RenewalStatus.ACTIVE;
                    modified = true;
                }
                subscription.LastReminderDate = DateTime.UtcNow;
                subscription.NextReminderDate = CalculateReminder(subscription.BillingCycle);
                subscription.CancellationDate = null;
                subscription.CancellationReason = null;
                modified = true;
            }
            else
            {
                subscription.RenewalStatus = RenewalStatus.SUSPENDED;
                subscription.NextReminderDate = DateTime.UtcNow.AddDays(1);
                modified = true;
            }

            if (modified)
            {
                subscription.DateEdit = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task HandlePaymentMethodEventAsync(Event stripeEvent, bool attached, CancellationToken cancellationToken)
        {
            var paymentMethod = DeserializeEntity<PaymentMethod>(stripeEvent);
            if (paymentMethod == null)
            {
                _logger.LogWarning("PaymentMethod event {EventId} missing payload.", stripeEvent.Id);
                return;
            }

            var customerId = paymentMethod.CustomerId ?? paymentMethod.Customer?.Id;
            if (string.IsNullOrWhiteSpace(customerId))
            {
                _logger.LogWarning("PaymentMethod event {EventId} missing customer reference.", stripeEvent.Id);
                return;
            }

            await _stripeSyncService.SyncCustomerFromStripeAsync(customerId, cancellationToken).ConfigureAwait(false);

            var company = await _dbContext.Company_DS
                .FirstOrDefaultAsync(c => c.StripeCustomerID == customerId, cancellationToken)
                .ConfigureAwait(false);

            if (company == null)
            {
                return;
            }

            var modified = false;
            if (!attached && string.Equals(company.StripeDefaultPaymentMethodID, paymentMethod.Id, StringComparison.Ordinal))
            {
                company.StripeDefaultPaymentMethodID = null;
                modified = true;
            }

            if (modified)
            {
                company.DateEdit = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task HandleInvoiceEventAsync(Event stripeEvent, InvoiceEventKind kind, CancellationToken cancellationToken)
        {
            var invoice = DeserializeEntity<Invoice>(stripeEvent);
            if (invoice == null)
            {
                _logger.LogWarning("Invoice event {EventId} missing payload.", stripeEvent.Id);
                return;
            }

            if (!string.IsNullOrWhiteSpace(invoice.CustomerId))
            {
                await _stripeSyncService.SyncCustomerFromStripeAsync(invoice.CustomerId, cancellationToken).ConfigureAwait(false);
            }

            var raw = stripeEvent.Data.RawJObject;
            var subscriptionId = raw?["subscription"]?.Value<string>();

            CompanySubscriptionModel? subscription = null;
            if (!string.IsNullOrWhiteSpace(subscriptionId))
            {
                subscription = await _stripeSyncService.SyncSubscriptionFromStripeAsync(subscriptionId, cancellationToken).ConfigureAwait(false);
            }

            subscription ??= await FindSubscriptionAsync(subscriptionId, invoice.Id, invoice.CustomerId, cancellationToken).ConfigureAwait(false);

            if (subscription == null)
            {
                _logger.LogInformation("Invoice {InvoiceId} processed but no local subscription was matched.", invoice.Id);
                return;
            }

            var modified = false;
            if (!string.IsNullOrWhiteSpace(invoice.Id) && !string.Equals(subscription.StripeInvoiceID, invoice.Id, StringComparison.Ordinal))
            {
                subscription.StripeInvoiceID = invoice.Id;
                modified = true;
            }

            switch (kind)
            {
                case InvoiceEventKind.Finalized:
                    subscription.NextReminderDate = DateTime.UtcNow.AddDays(1);
                    modified = true;
                    break;
                case InvoiceEventKind.Paid:
                    subscription.RenewalStatus = RenewalStatus.ACTIVE;
                    subscription.CancellationDate = null;
                    subscription.CancellationReason = null;
                    subscription.LastReminderDate = DateTime.UtcNow;
                    subscription.NextReminderDate = CalculateReminder(subscription.BillingCycle);
                    modified = true;
                    break;
                case InvoiceEventKind.PaymentFailed:
                    subscription.RenewalStatus = RenewalStatus.SUSPENDED;
                    subscription.NextReminderDate = DateTime.UtcNow.AddDays(1);
                    modified = true;
                    break;
                case InvoiceEventKind.PaymentActionRequired:
                    subscription.RenewalStatus = RenewalStatus.PAUSED;
                    subscription.NextReminderDate = DateTime.UtcNow.AddDays(1);
                    modified = true;
                    break;
            }

            if (modified)
            {
                subscription.DateEdit = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private void HandleInformationalEvent(Event stripeEvent)
        {
            _logger.LogInformation("Received Stripe event {EventType} (ID: {EventId}). Deferred to manual reconciliation/logging.", stripeEvent.Type, stripeEvent.Id);
        }

        private T? DeserializeEntity<T>(Event stripeEvent) where T : StripeEntity
        {
            if (stripeEvent.Data.Object is T typed)
            {
                return typed;
            }

            var rawObject = stripeEvent.Data.RawJObject;
            if (rawObject != null)
            {
                try
                {
                    return rawObject.ToObject<T>();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize Stripe payload for event {EventId} into {Type}.", stripeEvent.Id, typeof(T).Name);
                }
            }

            return null;
        }

        private async Task<CompanySubscriptionModel?> FindSubscriptionAsync(string? subscriptionId, string? invoiceId, string? customerId, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(subscriptionId))
            {
                var bySubscription = await _dbContext.Company_Subscription_DS
                    .FirstOrDefaultAsync(s => s.StripeSubscriptionID == subscriptionId, cancellationToken)
                    .ConfigureAwait(false);
                if (bySubscription != null)
                {
                    return bySubscription;
                }
            }

            if (!string.IsNullOrWhiteSpace(invoiceId))
            {
                var byInvoice = await _dbContext.Company_Subscription_DS
                    .FirstOrDefaultAsync(s => s.StripeInvoiceID == invoiceId, cancellationToken)
                    .ConfigureAwait(false);
                if (byInvoice != null)
                {
                    return byInvoice;
                }
            }

            if (!string.IsNullOrWhiteSpace(customerId))
            {
                var company = await _dbContext.Company_DS
                    .FirstOrDefaultAsync(c => c.StripeCustomerID == customerId, cancellationToken)
                    .ConfigureAwait(false);

                if (company != null)
                {
                    return await _dbContext.Company_Subscription_DS
                        .Where(s => s.TenantID == company.TenantID)
                        .OrderByDescending(s => s.DateIns)
                        .FirstOrDefaultAsync(cancellationToken)
                        .ConfigureAwait(false);
                }
            }

            return null;
        }

        private static DateTime CalculateReminder(BillingCycle cycle)
        {
            return cycle switch
            {
                BillingCycle.YEARLY => DateTime.UtcNow.AddDays(30),
                _ => DateTime.UtcNow.AddDays(7)
            };
        }

        private static DateTime NormalizeToUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };
        }

        private enum InvoiceEventKind
        {
            Created,
            Finalized,
            Paid,
            PaymentFailed,
            PaymentActionRequired
        }
    }
}
