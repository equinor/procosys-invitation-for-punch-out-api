using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.BusReceiver.Receiver.Interfaces;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.BusReceiver.Receiver
{
    
    public class PcsBusReceiver : IHostedService
    {
        private readonly ILogger<PcsBusReceiver> _logger;
        private readonly IPcsSubscriptionClients _subscriptionClients;
        private readonly IBusReceiverServiceFactory _busReceiverServiceFactory;

        public PcsBusReceiver(
            ILogger<PcsBusReceiver> logger,
            IPcsSubscriptionClients subscriptionClients,
            IBusReceiverServiceFactory busReceiverServiceFactory)
        {
            _logger = logger;
            _subscriptionClients = subscriptionClients;
            _busReceiverServiceFactory = busReceiverServiceFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };

            _subscriptionClients.RegisterPcsMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
            return Task.CompletedTask;
        }

        public async Task ProcessMessagesAsync(IPcsSubscriptionClient subscriptionClient, Message message, CancellationToken token)
        {
            try
            {
                var busReceiverService = _busReceiverServiceFactory.GetServiceInstance();

                await busReceiverService.ProcessMessageAsync(subscriptionClient.PcsTopic, message, token);
                
                await subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _subscriptionClients.CloseAllAsync();

            return Task.CompletedTask;
        }

        // Use this handler to examine the exceptions received on the message pump.
        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            _logger.LogError($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            _logger.LogError("Exception context for troubleshooting:");
            _logger.LogError($"- Endpoint: {context.Endpoint}");
            _logger.LogError($"- Entity Path: {context.EntityPath}");
            _logger.LogError($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }
    }
}
