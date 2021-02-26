using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.BusReceiver.Receiver.Interfaces;
using Microsoft.Azure.ServiceBus;

namespace Equinor.ProCoSys.BusReceiver.Receiver
{
    public class PcsSubscriptionClient : SubscriptionClient, IPcsSubscriptionClient
    {
        public PcsTopic PcsTopic { get; }

        private Func<IPcsSubscriptionClient, Message, CancellationToken, Task> _pcsHandler;
        public PcsSubscriptionClient(string connectionString, PcsTopic pcsTopic, string subscriptionName)
            : base(connectionString, pcsTopic.ToString(), subscriptionName, ReceiveMode.PeekLock, RetryPolicy.Default) =>
            PcsTopic = pcsTopic;

        public void RegisterPcsMessageHandler(Func<IPcsSubscriptionClient, Message, CancellationToken, Task> handler, MessageHandlerOptions messageHandlerOptions)
        {
            _pcsHandler = handler;
            base.RegisterMessageHandler(HandleMessage, messageHandlerOptions);
        }

        private Task HandleMessage(Message message, CancellationToken token) => _pcsHandler.Invoke(this, message, token);
    }
}
