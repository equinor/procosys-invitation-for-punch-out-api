using System;
using Equinor.ProCoSys.BusReceiver.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Equinor.ProCoSys.BusReceiver
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
