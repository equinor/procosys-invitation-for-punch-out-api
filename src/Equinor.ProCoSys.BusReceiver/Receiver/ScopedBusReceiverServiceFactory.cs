using System;
using Equinor.ProCoSys.BusReceiver.Receiver.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Equinor.ProCoSys.BusReceiver.Receiver
{
    public class ScopedBusReceiverServiceFactory : IBusReceiverServiceFactory
    {
        private readonly IServiceProvider _services;

        public ScopedBusReceiverServiceFactory(IServiceProvider services) => _services = services;

        public IBusReceiverService GetServiceInstance()
        {
            var scope = _services.CreateScope();
            var busReceiverService = scope.ServiceProvider.GetRequiredService<IBusReceiverService>();

            return busReceiverService;
        }
    }
}
