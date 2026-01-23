namespace Shared.Options
{
    public class SlackOptions
    {
        public string WebhookUrl { get; set; } = default!;
        public string DefaultChannel { get; set; } = "#general";
        public int TimeoutSeconds { get; set; } = 5;
    }
}
