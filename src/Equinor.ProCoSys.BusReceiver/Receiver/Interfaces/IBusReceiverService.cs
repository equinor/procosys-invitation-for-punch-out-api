using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace Equinor.ProCoSys.PcsBus.Receiver.Interfaces
{
    public interface IBusReceiverService
    {
        Task ProcessMessageAsync(PcsTopic pcsTopic, Message message, CancellationToken token);
    }
}
