using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.Events.PostSave;
using Equinor.ProCoSys.PcsServiceBus.Sender.Interfaces;
using Equinor.ProCoSys.PcsServiceBus.Topics;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.PostSaveEvents
{
    public class IpoCompletedEventHandler : INotificationHandler<IpoCompletedEvent>
    {
        private readonly IPcsBusSender _pcsBusSender;

        public IpoCompletedEventHandler(IPcsBusSender pcsBusSender)
        {
            _pcsBusSender = pcsBusSender;
        }

        public async Task Handle(IpoCompletedEvent notification, CancellationToken cancellationToken)
        {
            await SendBusTopicAsync(notification);
        }

        private async Task SendBusTopicAsync(IpoCompletedEvent notification)
        {
            var eventMessage = new BusEventMessage
            {
                Plant = notification.Plant,
                Event = "Completed",
                InvitationGuid = notification.SourceGuid
            };

            await _pcsBusSender.SendAsync(IpoTopic.TopicName, JsonSerializer.Serialize(eventMessage));
        }
    }
}
