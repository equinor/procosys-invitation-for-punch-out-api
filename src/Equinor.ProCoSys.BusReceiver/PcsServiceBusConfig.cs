using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Equinor.ProCoSys.BusReceiver
{
    public class PcsServiceBusConfig
    {
        public PcsServiceBusConfig(IServiceCollection services)
        {
            this.Services = services;
        }

        public IServiceCollection Services { get; }

        public PcsServiceBusConfig UseBusConnection(string connectionString)
        {
            ConnectionString = connectionString;
            return this;
        }

        public string ConnectionString { get; set; }

        public List<KeyValuePair<PcsTopic, string>> Subscriptions { get; set; } = new List<KeyValuePair<PcsTopic, string>>();

        public PcsServiceBusConfig WithSubscription(PcsTopic pcsTopic, string subscriptionName)
        {
            Subscriptions.Add(new KeyValuePair<PcsTopic, string>(pcsTopic, subscriptionName));
            return this;
        }
    }
}
