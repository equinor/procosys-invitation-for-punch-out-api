using Azure.Core;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Equinor.ProCoSys.IPO.WebApi.Extensions;

public static class ConfigureTelemetryExtension
{
    public static void ConfigureTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        TokenCredential credential)
    {
        services.AddApplicationInsightsTelemetry(options =>
        {
            options.ConnectionString = configuration["ConnectionStrings:ApplicationInsights"];
        });

        services.Configure<TelemetryConfiguration>(config =>
        {
            config.SetAzureTokenCredential(credential);
        });
    }
}
