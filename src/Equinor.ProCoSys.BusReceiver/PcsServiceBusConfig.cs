using System.Collections.Generic;

namespace Equinor.ProCoSys.PcsBus
{
    public class PcsServiceBusConfig
    {
        public PcsServiceBusConfig UseBusConnection(string connectionString)
        {
            ConnectionString = connectionString;
            return this;
        }

        public string ConnectionString { get; set; }

        public List<KeyValuePair<PcsTopic, string>> Subscriptions { get; } = new List<KeyValuePair<PcsTopic, string>>();

        public PcsServiceBusConfig WithSubscription(PcsTopic pcsTopic, string subscriptionName)
        {
            Subscriptions.Add(new KeyValuePair<PcsTopic, string>(pcsTopic, subscriptionName));
            return this;
        }
    }
}
