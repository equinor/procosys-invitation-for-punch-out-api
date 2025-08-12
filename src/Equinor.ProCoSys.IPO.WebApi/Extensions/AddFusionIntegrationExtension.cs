using System;
using Equinor.ProCoSys.Common.Misc;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Equinor.ProCoSys.IPO.WebApi.Extensions;

public static class AddFusionIntegrationExtension
{
    public static void AddFusionIntegration(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var environment = builder.Environment;
        
        if (environment.IsIntegrationTest())
        {
            return;
        }
        
        builder.Services.AddFusionIntegration(options =>
        {
            options.UseServiceInformation("PCS IPO", environment.EnvironmentName); // Environment identifier
            options.UseDefaultEndpointResolver(
                configuration
                    ["Meetings:Environment"]); // Fusion environment "fprd" = prod, "fqa" = qa, "ci" = dev/test etc
            options.UseDefaultTokenProvider(opts =>
            {
                opts.ClientId = configuration["Meetings:ClientId"]; // Application client ID
                opts.ClientSecret = configuration["Meetings:ClientSecret"]; // Application client secret
            });
            options.AddMeetings(s => s.SetHttpClientTimeout(
                TimeSpan.FromSeconds(configuration.GetValue<double>("FusionRequestTimeout")),
                TimeSpan.FromSeconds(configuration.GetValue<double>("FusionTotalTimeout"))));
            options.DisableClaimsTransformation(); // Disable this - Fusion adds relevant claims
        });
    }
}
