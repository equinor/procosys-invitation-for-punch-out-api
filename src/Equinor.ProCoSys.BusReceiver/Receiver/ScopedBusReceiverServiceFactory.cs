using System;
using Equinor.ProCoSys.PcsBus.Receiver.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Equinor.ProCoSys.PcsBus.Receiver
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
