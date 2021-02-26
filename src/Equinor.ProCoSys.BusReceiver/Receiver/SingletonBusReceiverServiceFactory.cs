using Equinor.ProCoSys.BusReceiver.Receiver.Interfaces;

namespace Equinor.ProCoSys.BusReceiver.Receiver
{
    public class SingletonBusReceiverServiceFactory : IBusReceiverServiceFactory
    {
        private readonly IBusReceiverService _service;

        public SingletonBusReceiverServiceFactory(IBusReceiverService service) => _service = service;

        public IBusReceiverService GetServiceInstance() => _service;
    }
}
