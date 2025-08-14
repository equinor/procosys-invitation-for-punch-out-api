using System;
using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace Equinor.ProCoSys.IPO.WebApi.Extensions;

public static class ConfigureAzureAppConfigExtension
{
    public static void ConfigureAzureAppConfig(this WebApplicationBuilder builder, TokenCredential credential)
    {
        var configuration = builder.Configuration;

        // TODO
        // if (!configuration.GetValue<bool>("Application:UseAzureAppConfiguration"))
        // {
        //     return;
        // }

        configuration.AddAzureAppConfiguration(options =>
        {
            // TODO remove debug 
            var appConfigUrl = "https://pcs-ipo-non-prod-config.azconfig.io";
            // var appConfigUrl = configuration["Application:AppConfigurationUrl"]!;

            options.Connect(new Uri(appConfigUrl), credential)
                .ConfigureKeyVault(kv =>
                {
                    kv.SetCredential(credential);
                })
                .Select(KeyFilter.Any)
                .Select(KeyFilter.Any, builder.Environment.EnvironmentName)
                .ConfigureRefresh(refreshOptions =>
                {
                    refreshOptions.Register("Sentinel", true);
                    refreshOptions.SetRefreshInterval(TimeSpan.FromSeconds(30));
                });
        });
    }
}
