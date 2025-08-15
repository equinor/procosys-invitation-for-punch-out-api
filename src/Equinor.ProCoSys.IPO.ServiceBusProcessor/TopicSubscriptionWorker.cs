using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.IPO.ServiceBusProcessor;

public abstract class TopicSubscriptionWorker<TMessage>(Azure.Messaging.ServiceBus.ServiceBusProcessor processor, ILogger<TopicSubscriptionWorker<TMessage>> logger) : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        processor.ProcessMessageAsync += HandleMessageAsync;
        processor.ProcessErrorAsync += HandleReceivedExceptionAsync;

        logger.LogInformation($"Starting message pump on topic {processor.EntityPath} in namespace {processor.FullyQualifiedNamespace}");
        await processor.StartProcessingAsync(stoppingToken);
        logger.LogInformation("Message pump started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        logger.LogInformation("Closing message pump");
        await processor.CloseAsync(CancellationToken.None);
        await processor.DisposeAsync();
        logger.LogInformation("Message pump closed : {Time}", DateTimeOffset.UtcNow);
    }


    private async Task HandleMessageAsync (ProcessMessageEventArgs processMessageEventArgs)
    {
        try
        {
            var rawMessageBody = Encoding.UTF8.GetString(processMessageEventArgs.Message.Body);
            logger.LogInformation("Received message {MessageId} with body {MessageBody}",
                processMessageEventArgs.Message.MessageId, rawMessageBody);

            // TODO handle message

            await processMessageEventArgs.CompleteMessageAsync(processMessageEventArgs.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to handle message");
        }
    }

    private Task HandleReceivedExceptionAsync(ProcessErrorEventArgs exceptionEvent)
    {
        logger.LogError(exceptionEvent.Exception, "Unable to process message");
        return Task.CompletedTask;
    }

    protected abstract Task ProcessMessage(TMessage message, string messageId, IReadOnlyDictionary<string, object> userProperties, CancellationToken cancellationToken);
}
