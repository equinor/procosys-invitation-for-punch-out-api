using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.Events.PostSave;
using MediatR;
using Microsoft.Azure.ServiceBus;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.PostSaveEvents
{
    public class IpoCanceledEventHandler : INotificationHandler<IpoCanceledEvent>
    {
        private readonly ITopicClient _topicClient;

        public IpoCanceledEventHandler(ITopicClient topicClient) => _topicClient = topicClient;

        public async Task Handle(IpoCanceledEvent notification, CancellationToken cancellationToken)
        {
            var eventMessage = new BusEventMessage
            {
                ProjectSchema = notification.Plant,
                Event = "Canceled",
                InvitationGuid = notification.ObjectGuid,
                IpoStatus = notification.Status
            };
            var message = new Message(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(eventMessage)));
            //var topicClient =
            //    new TopicClient(
            //        "Endpoint=sb://sb-pcs-dev.servicebus.windows.net/;SharedAccessKeyName=ListenSend;SharedAccessKey=MLR0H56O+QCUNbSYygBHgPYF0Ocz199UCfuCoo1x5VE=",
            //        "ipo");

            await _topicClient.SendAsync(message);
        }
    }
}
