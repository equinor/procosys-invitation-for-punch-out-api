using Equinor.ProCoSys.PcsBus.Receiver.Interfaces;

namespace Equinor.ProCoSys.PcsBus.Receiver
{
    public class SingletonBusReceiverServiceFactory : IBusReceiverServiceFactory
    {
        private readonly IBusReceiverService _service;

        public SingletonBusReceiverServiceFactory(IBusReceiverService service) => _service = service;

        public IBusReceiverService GetServiceInstance() => _service;
    }
}
