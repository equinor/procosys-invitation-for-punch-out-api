using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.BusReceiver.Interfaces;
using Microsoft.Azure.ServiceBus;

namespace Equinor.ProCoSys.BusReceiver
{
    public class PcsSubscriptionClients : IPcsSubscriptionClients
    {
        private readonly List<IPcsSubscriptionClient> _subscriptionClients = new List<IPcsSubscriptionClient>();


        public void Add(IPcsSubscriptionClient pcsSubscriptionClient) => _subscriptionClients.Add(pcsSubscriptionClient);

        public async Task CloseAllAsync()
        {
            foreach (var s in _subscriptionClients)
            {
                await s.CloseAsync();
            }
        }

        public void RegisterPcsMessageHandler(
            Func<IPcsSubscriptionClient, Message, CancellationToken, Task> handler,
            MessageHandlerOptions options) =>
                _subscriptionClients.ForEach(s => s.RegisterPcsMessageHandler(handler, options));
    }
}
