using System;
using System.Text.Json.Serialization;
using Azure.Core;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.WebApi.MassTransit;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Equinor.ProCoSys.IPO.WebApi.Extensions;

public static class ConfigureMassTransitExtension
{
    public static void ConfigureMassTransit(this WebApplicationBuilder builder, TokenCredential credential) =>
        builder.Services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<IPOContext>(o =>
            {
                o.UseSqlServer();
                o.UseBusOutbox();
            });

            if (!builder.IsServiceBusEnabled())
            {
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });

                return;
            }

            x.UsingAzureServiceBus((_, cfg) =>
            {
                var serviceBusNamespace = builder.Configuration.GetValue<string>("ServiceBus:Namespace");
                var serviceUri = new Uri($"sb://{serviceBusNamespace}.servicebus.windows.net/");

                cfg.Host(serviceUri, host =>
                {
                    host.TokenCredential = credential;
                });

                cfg.MessageTopology.SetEntityNameFormatter(new IpoEntityNameFormatter());
                cfg.UseRawJsonSerializer();
                cfg.ConfigureJsonSerializerOptions(opts =>
                {
                    opts.Converters.Add(new JsonStringEnumConverter());

                    // Set it to null to use the default .NET naming convention (PascalCase)
                    opts.PropertyNamingPolicy = null;
                    return opts;
                });

                cfg.AutoStart = true;
            });
        });
}
