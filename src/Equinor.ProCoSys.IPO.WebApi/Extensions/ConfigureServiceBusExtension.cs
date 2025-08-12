using System;
using Equinor.ProCoSys.IPO.WebApi.Synchronization;
using Equinor.ProCoSys.PcsServiceBus;
using Equinor.ProCoSys.PcsServiceBus.Sender.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Equinor.ProCoSys.IPO.WebApi.Extensions;

public static class ConfigureServiceBusExtension
{
    public static void ConfigureServiceBus(this WebApplicationBuilder builder)
    {
        if (!builder.IsServiceBusEnabled())
        {
            builder.Services.AddSingleton<IPcsBusSender>(new DisabledServiceBusSender());
            return;
        }
        
        var configuration = builder.Configuration;
        
        // Env variable used in kubernetes. Configuration is added for easier use locally
        // Url will be validated during startup of service bus integration and give a
        // Uri exception if invalid.
        var leaderElectorUrl = Environment.GetEnvironmentVariable("LEADERELECTOR_SERVICE") ?? (configuration["ServiceBus:LeaderElectorUrl"]);

        builder.Services.AddPcsServiceBusIntegration(options => options
            .UseBusConnection(configuration.GetConnectionString("ServiceBus"))
            .WithLeaderElector(leaderElectorUrl)
            .WithRenewLeaseInterval(int.Parse(configuration["ServiceBus:LeaderElectorRenewLeaseInterval"]))
            .WithSubscription(PcsTopicConstants.Ipo, "ipo_ipo")
            .WithSubscription(PcsTopicConstants.Project, "ipo_project")
            .WithSubscription(PcsTopicConstants.CommPkg, "ipo_commpkg")
            .WithSubscription(PcsTopicConstants.McPkg, "ipo_mcpkg")
            .WithSubscription(PcsTopicConstants.Library, "ipo_library")
            .WithSubscription(PcsTopicConstants.Certificate, "ipo_certificate")
            //THIS METHOD SHOULD BE FALSE IN NORMAL OPERATION.
            //ONLY SET TO TRUE WHEN A LARGE NUMBER OF MESSAGES HAVE FAILED AND ARE COPIED TO DEAD LETTER.
            //WHEN SET TO TRUE, MESSAGES ARE READ FROM DEAD LETTER QUEUE INSTEAD OF NORMAL QUEUE
            .WithReadFromDeadLetterQueue(configuration.GetValue("ServiceBus:ReadFromDeadLetterQueue", defaultValue: false)));

        var topics = configuration["ServiceBus:TopicNames"];
        builder.Services.AddTopicClients(configuration.GetConnectionString("ServiceBus"), topics);
    }
    
    private static bool IsServiceBusEnabled(this WebApplicationBuilder builder) =>
        builder.Configuration.GetValue<bool>("ServiceBus:Enable") &&
        (!builder.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("ServiceBus:EnableInDevelopment"));
}
