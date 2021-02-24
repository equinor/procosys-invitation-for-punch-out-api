using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.Events.PostSave;
using MediatR;
using Microsoft.Azure.ServiceBus;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.PostSaveEvents
{
    public class IpoUnAcceptedEventHandler : INotificationHandler<IpoUnAcceptedEvent>
    {
        private readonly ITopicClient _topicClient;

        public IpoUnAcceptedEventHandler(ITopicClient topicClient) => _topicClient = topicClient;

        public async Task Handle(IpoUnAcceptedEvent notification, CancellationToken cancellationToken)
        {
            var eventMessage = new BusEventMessage
            {
                ProjectSchema = notification.Plant,
                Event = "UnAccepted",
                InvitationGuid = notification.ObjectGuid
            };

            var message = new Message(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(eventMessage)));
            // TODO: Remove
            //var topicClient =
            //    new TopicClient(
            //        "Endpoint=sb://sb-pcs-dev.servicebus.windows.net/;SharedAccessKeyName=ListenSend;SharedAccessKey=MLR0H56O+QCUNbSYygBHgPYF0Ocz199UCfuCoo1x5VE=",
            //        "ipo");

            await _topicClient.SendAsync(message);
        }
    }
}
