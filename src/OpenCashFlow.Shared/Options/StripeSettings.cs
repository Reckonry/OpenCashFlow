namespace Shared.Options
{
    public class StripeSettings
    {
        public required string SecretKey { get; set; }
        public required string PublishableKey { get; set; }
        public required string WebhookSecret { get; set; }
        public string? DashboardUrl { get; set; }
    }
}
