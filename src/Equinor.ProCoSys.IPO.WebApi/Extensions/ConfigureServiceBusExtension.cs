using System;
using Azure.Core;
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
    public static void ConfigureServiceBus(this WebApplicationBuilder builder, TokenCredential credential)
    {
        if (!builder.IsServiceBusEnabled())
        {
            builder.Services.AddSingleton<IPcsBusSender>(new DisabledServiceBusSender());
            return;
        }

        var configuration = builder.Configuration;
        var fullyQualifiedNamespace = $"{configuration["ServiceBus:Namespace"]}.servicebus.windows.net";

        builder.Services.AddPcsServiceBusIntegration(options => options
            .UseCredentialAuthentication(fullyQualifiedNamespace, credential)
            .WithLeaderElector(configuration["ServiceBus:LeaderElectorUrl"]!)
            .WithRenewLeaseInterval(int.Parse(configuration["ServiceBus:LeaderElectorRenewLeaseInterval"]!))
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
        builder.Services.AddTopicClients(topics.Split(','), fullyQualifiedNamespace, credential);
    }

    private static bool IsServiceBusEnabled(this WebApplicationBuilder builder) =>
        builder.Configuration.GetValue<bool>("ServiceBus:Enable") &&
        (!builder.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("ServiceBus:EnableInDevelopment"));
}
