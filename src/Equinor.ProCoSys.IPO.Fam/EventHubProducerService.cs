using System.Text;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Equinor.ProCoSys.IPO.Fam;

public class EventHubProducerService : IEventHubProducerService
{
    private readonly IFamCredential _credential;
    private readonly string _fullyQualifiedNamespace;
    private readonly string _eventHubName;

    public EventHubProducerService(IFamCredential credential, IConfiguration config)
    {
        _credential = credential;

        var eventHubNamespace = config.GetValue<string>("Fam:EventHub:Namespace")!;
        _fullyQualifiedNamespace = $"{eventHubNamespace}.servicebus.windows.net";

        _eventHubName = config.GetValue<string>("Fam:EventHub:EventHubName")!;
    }

    public async Task SendDataAsync<T>(IEnumerable<T> data)
    {
        var token = _credential.GetToken();

        await using var producerClient = new EventHubProducerClient(_fullyQualifiedNamespace, _eventHubName, token);

        await SendDataAsync(data, producerClient);
    }


    private static async Task SendDataAsync<T>(IEnumerable<T> data, EventHubProducerClient producerClient)
    {
        var eventData = data.Select(CreateEventData).ToList();

        var i = 0;
        while (i < eventData.Count)
        {
            using var eventBatch = await producerClient.CreateBatchAsync();
            for (; i < eventData.Count; i++)
            {
                if (!eventBatch.TryAdd(eventData[i]))
                {
                    break;
                }
            }

            await producerClient.SendAsync(eventBatch);
        }
    }

    private static EventData CreateEventData<T>(T data)
    {
        var dataAsJson = JsonConvert.SerializeObject(data);
        return new EventData(Encoding.UTF8.GetBytes(dataAsJson));
    }
}
