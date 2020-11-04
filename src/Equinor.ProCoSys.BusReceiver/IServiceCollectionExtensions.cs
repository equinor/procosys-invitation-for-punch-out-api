using System;
using Equinor.ProCoSys.BusReceiver.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Equinor.ProCoSys.BusReceiver
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddPcsServiceBusIntegration(this IServiceCollection services, Action<PcsServiceBusConfig> options)
        {
            var optionsBuilder = new PcsServiceBusConfig(services);
            options(optionsBuilder);

            var pcsSubscriptionClients = new PcsSubscriptionClients();
            optionsBuilder.Subscriptions.ForEach(
                s => 
                    pcsSubscriptionClients.Add(
                        new PcsSubscriptionClient(optionsBuilder.ConnectionString, s.Key, s.Value)));
            services.AddSingleton<IPcsSubscriptionClients>(pcsSubscriptionClients);

            services.AddHostedService<PcsBusReceiver>();

            return services;
        }
    }
}
