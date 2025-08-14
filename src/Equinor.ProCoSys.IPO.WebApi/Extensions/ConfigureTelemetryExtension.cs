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
            // TODO remove debug
            options.ConnectionString = configuration["InstrumentationKey=298df54f-eae4-4bee-a645-678c3b594883;IngestionEndpoint=https://northeurope-3.in.applicationinsights.azure.com/;LiveEndpoint=https://northeurope.livediagnostics.monitor.azure.com/;ApplicationId=acf2701d-97da-47c4-bab0-c6ce626e5c97"];
            // options.ConnectionString = configuration["ApplicationInsights:ConnectionString"];
        });

        services.Configure<TelemetryConfiguration>(config =>
        {
            config.SetAzureTokenCredential(credential);
        });
    }
}
