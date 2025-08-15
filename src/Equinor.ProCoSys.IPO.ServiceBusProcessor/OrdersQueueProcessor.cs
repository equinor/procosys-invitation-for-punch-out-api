using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.IPO.ServiceBusProcessor;

public class OrdersTopicProcessor(Azure.Messaging.ServiceBus.ServiceBusProcessor processor, ILogger<OrdersTopicProcessor> logger) : TopicSubscriptionWorker<Order>(processor, logger)
{
    protected override async Task ProcessMessage(Order order, string messageId, IReadOnlyDictionary<string, object> userProperties, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing order {OrderId} for {OrderAmount} units of {OrderArticle} bought by {CustomerFirstName} {CustomerLastName}", order.Id, order.Amount, order.ArticleNumber, order.Customer.FirstName, order.Customer.LastName);

        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

        logger.LogInformation("Order {OrderId} processed", order.Id);
    }
}
