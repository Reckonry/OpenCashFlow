namespace OpenCashFlow.Api.AppStart;

public static class CorsAppStart
{
    public const string PolicyName = "AllowAPIService";

    public static WebApplicationBuilder AppStartConfigureCors(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policyBuilder =>
            {
                var originsFromConfig = builder.Configuration
                    .GetSection("Cors:AllowedOrigins")
                    .Get<string[]>() ?? Array.Empty<string>();

                var origins = originsFromConfig.Length > 0
                    ? originsFromConfig
                    : new[] { builder.Configuration["AppUrl"] ?? string.Empty };

                origins = origins
                    .Where(o => !string.IsNullOrWhiteSpace(o))
                    .Select(o => o.TrimEnd('/'))
                    .ToArray();

                policyBuilder
                    .WithOrigins(origins)
                    .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                    .WithHeaders("Authorization", "Content-Type", "Accept", "X-Requested-With", "X-SignalR-User-Agent")
                    .SetPreflightMaxAge(TimeSpan.FromMinutes(10));

                if (builder.Configuration.GetValue<bool>("Cors:AllowCredentials"))
                {
                    policyBuilder.AllowCredentials();
                }
            });
        });

        return builder;
    }
}