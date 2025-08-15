using System.ComponentModel.DataAnnotations;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.ServiceBusProcessor;

public static class AddServiceBusClientExtension
{
    public static IServiceCollection AddTopicSubscriptionServices(this IServiceCollection services)
    {
        services.AddSingleton<ServiceBusClient>(svc =>
        {
            var logger = svc.GetRequiredService<ILogger<ServiceBusClient>>();
            var options = svc.GetRequiredService<IOptions<TopicSubscriptionOptions>>();
            return new ServiceBusClient(options.Value.FullyQualifiedNamespace, new DefaultAzureCredential());
        });
        services.AddScoped<ServiceBusReceiver>(svc =>
        {
            var client = svc.GetRequiredService<ServiceBusClient>();
            var options = svc.GetRequiredService<IOptions<TopicSubscriptionOptions>>();
            return client.CreateReceiver(options.Value.TopicName, options.Value.SubscriptionName);
        });
        services.AddScoped<ServiceBusSender>(svc =>
        {
            var client = svc.GetRequiredService<ServiceBusClient>();
            var options = svc.GetRequiredService<IOptions<TopicSubscriptionOptions>>();
            return client.CreateSender(options.Value.TopicName);
        });
        services.AddScoped<Azure.Messaging.ServiceBus.ServiceBusProcessor>(svc =>
        {
            var client = svc.GetRequiredService<ServiceBusClient>();
            var options = svc.GetRequiredService<IOptions<TopicSubscriptionOptions>>();
            return client.CreateProcessor(options.Value.TopicName, options.Value.SubscriptionName);
        });

        return services;
    }
}

public class TopicSubscriptionOptions
{
    [Required]
    public string TopicName { get; set; }
    
    [Required]
    public string SubscriptionName { get; set; }
    
    [Required]
    public string FullyQualifiedNamespace { get; set;}
}
