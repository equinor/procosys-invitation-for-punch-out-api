using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.Events.PostSave;
using Equinor.ProCoSys.PcsServiceBus.Sender.Interfaces;
using Equinor.ProCoSys.PcsServiceBus.Topics;
using MediatR;

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
                Plant = notification.Plant,
                Event = "Canceled",
                InvitationGuid = notification.SourceGuid,
                IpoStatus = notification.Status
            };
            
            //await _pcsBusSender.SendAsync(IpoTopic.TopicName, JsonSerializer.Serialize(eventMessage));
        }
    }
}
