using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Email;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Domain.Events.PostSave;
using Equinor.ProCoSys.PcsServiceBus.Sender.Interfaces;
using Equinor.ProCoSys.PcsServiceBus.Topics;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.PostSaveEvents
{
    public class IpoCompletedEventHandler : INotificationHandler<IpoCompletedEvent>
    {
        private readonly IIntegrationEventPublisher _publisher;

        public IpoCompletedEventHandler(IIntegrationEventPublisher publisher) => _publisher = publisher;

        public async Task Handle(IpoCompletedEvent notification, CancellationToken cancellationToken) => await SendBusTopicAsync(notification);

        private async Task SendBusTopicAsync(IpoCompletedEvent notification)
        {
            var eventMessage = new BusEventMessage
            {
                Plant = notification.Plant,
                Event = "Completed",
                InvitationGuid = notification.SourceGuid
            };

            //How is topicname handled? ==> Use an EntityNameFormatter, see Completion
            //TODO: JSOI Move this to command handler
            //await _publisher.PublishAsync(eventMessage, CancellationToken.None);
            //await _pcsBusSender.SendAsync(IpoTopic.TopicName, JsonSerializer.Serialize(eventMessage));
        }
    }
}
