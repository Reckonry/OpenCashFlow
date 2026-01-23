using Npgsql;

namespace OpenCashFlow.API.Helpers
{
    public static class RetryHelper
    {
        public static async Task<T?> ExecuteWithRetryAsync<T>(
            Func<Task<T>> operation,
            int maxAttempts = 3,
            Func<Exception, bool>? retryCondition = null,
            ILogger? logger = null,
            int baseDelayMilliseconds = 200)
        {
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    return await operation();
                }
                catch (Exception ex) when (retryCondition?.Invoke(ex) ?? false)
                {
                    logger?.LogWarning("Tentativo {Attempt}/{MaxAttempts} fallito con errore temporaneo: {Message}", attempt, maxAttempts, ex.Message);

                    if (attempt == maxAttempts)
                    {
                        logger?.LogError(ex, "Operazione fallita dopo {MaxAttempts} tentativi.", maxAttempts);
                        throw;
                    }

                    await Task.Delay(baseDelayMilliseconds * attempt);
                }
            }

            return default;
        }

        public static bool IsDeadlock(Exception ex) =>
            ex is PostgresException pgEx && pgEx.SqlState == "40P01";
    }
}
