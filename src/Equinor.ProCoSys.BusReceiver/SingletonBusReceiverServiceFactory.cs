using Equinor.ProCoSys.BusReceiver.Interfaces;

namespace Equinor.ProCoSys.BusReceiver
{
    public class SingletonBusReceiverServiceFactory : IBusReceiverServiceFactory
    {
        private readonly IBusReceiverService _service;

        public SingletonBusReceiverServiceFactory(IBusReceiverService service) => _service = service;

        public IBusReceiverService GetServiceInstance() => _service;
    }
}
