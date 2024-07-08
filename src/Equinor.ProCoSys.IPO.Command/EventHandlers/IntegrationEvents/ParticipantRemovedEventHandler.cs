using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Command.Events;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;

public class ParticipantRemovedEventHandler : INotificationHandler<ParticipantRemovedEvent>
{
    private readonly IIntegrationEventPublisher _integrationEventPublisher;

    public ParticipantRemovedEventHandler(IIntegrationEventPublisher integrationEventPublisher) => _integrationEventPublisher = integrationEventPublisher;

    public Task Handle(ParticipantRemovedEvent notification, CancellationToken cancellationToken)
    {
        var participantEvent = new ParticipantDeleteEvent { Plant = notification.Plant, ProCoSysGuid = notification.ParticipantGuid };
        return _integrationEventPublisher.PublishAsync(participantEvent, cancellationToken);
    }
}
