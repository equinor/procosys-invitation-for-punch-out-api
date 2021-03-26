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
    public class IpoCanceledEventHandler : INotificationHandler<IpoCanceledEvent>
    {
        private readonly IPcsBusSender _pcsBusSender;

        public IpoCanceledEventHandler(IPcsBusSender pcsBusSender) => _pcsBusSender = pcsBusSender;

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

            await _pcsBusSender.SendAsync(IpoTopic.TopicName, message);
        }
    }
}
