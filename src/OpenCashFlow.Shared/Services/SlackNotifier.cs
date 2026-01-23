using Microsoft.Extensions.Options;
using Shared.Options;
using Shared.Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace Shared.Services
{
    public class SlackNotifier : ISlackNotifier
    {
        private readonly HttpClient _http;
        private readonly SlackOptions _opts;

        public SlackNotifier(HttpClient http, IOptions<SlackOptions> opts)
        {
            _http = http;
            _opts = opts.Value;
        }

        public async Task NotifyAsync(string message, string? channel = null)
        {
            var payload = new
            {
                text = message,
                channel = channel ?? _opts.DefaultChannel
            };
            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(_opts.WebhookUrl, content);
            response.EnsureSuccessStatusCode();
        }
    }
}
