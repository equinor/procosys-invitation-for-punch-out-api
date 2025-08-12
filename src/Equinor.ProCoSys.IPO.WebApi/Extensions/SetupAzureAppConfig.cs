using System;
using Azure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace Equinor.ProCoSys.IPO.WebApi.Extensions;

public static class SetupAzureAppConfig
{
    public static void ConfigureAzureAppConfig(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        
        builder.Configuration.AddAzureAppConfiguration(options =>
        {
            var connectionString = configuration["ConnectionStrings:AppConfig"];
            options.Connect(connectionString)
                .ConfigureKeyVault(kv =>
                {
                    kv.SetCredential(new ManagedIdentityCredential());
                    // Use DefaultAzureCredential in local dev env.
                    // kv.SetCredential(new DefaultAzureCredential());
                })
                .Select(KeyFilter.Any)
                .Select(KeyFilter.Any, builder.Environment.EnvironmentName)
                .ConfigureRefresh(refreshOptions =>
                {
                    refreshOptions.Register("Sentinel", true);
                    refreshOptions.SetCacheExpiration(TimeSpan.FromSeconds(30));
                });
        });
    }
}
