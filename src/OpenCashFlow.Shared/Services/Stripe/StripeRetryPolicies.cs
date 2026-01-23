using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace Shared.Services.Stripe
{
    /// <summary>
    /// Factory centralizzata per definire le politiche Polly usate nello StripeService.
    /// </summary>
    public static class StripeRetryPolicies
    {
        private static readonly int[] RetryableStatusCodes = [408, 409, 429, 500, 502, 503, 504];
        private static readonly HashSet<string> RetryableErrorTypes =
        [
            "api_connection_error",
            "api_error",
            "rate_limit"
        ];

        public static AsyncRetryPolicy CreateAsyncRetryPolicy(Action<int, TimeSpan, Exception> onRetry)
        {
            var delay = Backoff.ExponentialBackoff(TimeSpan.FromMilliseconds(200), retryCount: 3);
            return Policy
                .Handle<StripeException>(IsTransientStripeError)
                .Or<HttpRequestException>()
                .WaitAndRetryAsync(delay, (exception, sleep, attempt, _) =>
                {
                    onRetry?.Invoke(attempt, sleep, exception);
                });
        }

        private static bool IsTransientStripeError(StripeException ex)
        {
            if (ex == null)
            {
                return false;
            }

            if (ex.StripeError == null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(ex.StripeError.Type) && RetryableErrorTypes.Contains(ex.StripeError.Type))
            {
                return true;
            }

            if (!string.IsNullOrEmpty(ex.StripeError.Code) && RetryableErrorTypes.Contains(ex.StripeError.Code))
            {
                return true;
            }

            if (ex.HttpStatusCode is HttpStatusCode status && RetryableStatusCodes.Contains((int)status))
            {
                return true;
            }

            return false;
        }
    }
}
