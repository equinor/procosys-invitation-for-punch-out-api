using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.Events.PostSave;
using Equinor.ProCoSys.PcsServiceBus.Sender.Interfaces;
using Equinor.ProCoSys.PcsServiceBus.Topics;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.PostSaveEvents
{
    public class IpoAcceptedEventHandler : INotificationHandler<IpoAcceptedEvent>
    {
        private readonly IPcsBusSender _pcsBusSender;

        public IpoAcceptedEventHandler(IPcsBusSender pcsBusSender) => _pcsBusSender = pcsBusSender;

        public async Task Handle(IpoAcceptedEvent notification, CancellationToken cancellationToken)
        {
            var eventMessage = new BusEventMessage
            {
                ProjectSchema = notification.Plant,
                Event = "Accepted",
                InvitationGuid = notification.ObjectGuid
            };

            await _pcsBusSender.SendAsync(IpoTopic.TopicName, JsonSerializer.Serialize(eventMessage));
        }
    }
}
