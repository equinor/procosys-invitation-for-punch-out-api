using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Domain.Events.PostSave;
using Equinor.ProCoSys.PcsServiceBus.Sender.Interfaces;
using Equinor.ProCoSys.PcsServiceBus.Topics;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.PostSaveEvents
{
    public class IpoUnCompletedEventHandler : INotificationHandler<IpoUnCompletedEvent>
    {
        private readonly IIntegrationEventPublisher _publisher;

        public IpoUnCompletedEventHandler(IIntegrationEventPublisher publisher) => _publisher = publisher;

        public async Task Handle(IpoUnCompletedEvent notification, CancellationToken cancellationToken)
        {
            var eventMessage = new BusEventMessage
            {
                Plant = notification.Plant,
                Event = "UnCompleted",
                InvitationGuid = notification.SourceGuid
            };

            //TODO: JSOI Move this to command handler
            //await _publisher.PublishAsync(eventMessage, CancellationToken.None);
            //await _pcsBusSender.SendAsync(IpoTopic.TopicName, JsonSerializer.Serialize(eventMessage));
        }
    }
}
