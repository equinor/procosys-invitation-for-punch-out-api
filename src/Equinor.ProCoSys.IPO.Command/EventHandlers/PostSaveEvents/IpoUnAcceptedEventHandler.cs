using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.Events.PostSave;
using Equinor.ProCoSys.PcsServiceBus.Sender.Interfaces;
using Equinor.ProCoSys.PcsServiceBus.Topics;
using MediatR;
using Microsoft.Azure.ServiceBus;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.PostSaveEvents
{
    public class IpoUnAcceptedEventHandler : INotificationHandler<IpoUnAcceptedEvent>
    {
        private readonly IPcsBusSender _pcsBusSender;

        public IpoUnAcceptedEventHandler(IPcsBusSender pcsBusSender) => _pcsBusSender = pcsBusSender;

        public async Task Handle(IpoUnAcceptedEvent notification, CancellationToken cancellationToken)
        {
            var eventMessage = new BusEventMessage
            {
                ProjectSchema = notification.Plant,
                Event = "UnAccepted",
                InvitationGuid = notification.ObjectGuid
            };

            await _pcsBusSender.SendAsync(IpoTopic.TopicName, JsonSerializer.Serialize(eventMessage));
        }
    }
}
