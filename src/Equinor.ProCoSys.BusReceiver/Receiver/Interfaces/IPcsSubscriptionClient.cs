using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace Equinor.ProCoSys.PcsBus.Receiver.Interfaces
{
    public interface IPcsSubscriptionClient
    {
        PcsTopic PcsTopic { get; }
        void RegisterPcsMessageHandler(Func<IPcsSubscriptionClient, Message, CancellationToken, Task> handler, MessageHandlerOptions messageHandlerOptions);
        Task CompleteAsync(string token);
        Task CloseAsync();
    }
}
