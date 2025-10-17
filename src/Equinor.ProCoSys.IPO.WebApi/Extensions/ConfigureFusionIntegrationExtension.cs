using System;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Command;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;

namespace Equinor.ProCoSys.IPO.WebApi.Extensions;

public static class ConfigureFusionIntegrationExtension
{
    public static void ConfigureFusionIntegration(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var environment = builder.Environment;

        if (environment.IsIntegrationTest())
        {
            return;
        }

        builder.Services.AddFusionIntegration(options =>
        {
            var meetingOptions = configuration.GetSection("Meetings");
            
            options.UseServiceInformation("PCS IPO", environment.EnvironmentName); // Environment identifier
            options.UseDefaultEndpointResolver(
                meetingOptions.GetValue<string>(nameof(MeetingOptions.Environment))); // Fusion environment "fprd" = prod, "fqa" = qa, "ci" = dev/test etc
            options.UseDefaultTokenProvider(opts =>
            {
                opts.ClientId = meetingOptions.GetValue<string>(nameof(MeetingOptions.ClientId)); // Application client ID
                // opts.ClientAssertion = _ =>
                // {
                //     var aksClientAssertion = new AzureIdentityForKubernetesClientAssertion();
                //     return aksClientAssertion.GetSignedAssertionAsync(
                //         new AssertionRequestOptions
                //         {
                //             ClientID = meetingOptions.GetValue<string>(nameof(MeetingOptions.ClientId))
                //         });
                // };
            });
            options.AddMeetings(s => s.SetHttpClientTimeout(
                TimeSpan.FromSeconds(configuration.GetValue<double>("FusionRequestTimeout")),
                TimeSpan.FromSeconds(configuration.GetValue<double>("FusionTotalTimeout"))));
            options.DisableClaimsTransformation(); // Disable this - Fusion adds relevant claims
        });
    }
}
