using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;
using System.Threading.Tasks;
using System.Threading;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;

public class ParticipantUpdatedEventHandler : INotificationHandler<ParticipantUpdatedEvent>
{
    private readonly IEventRepository _eventRepository;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;

    public ParticipantUpdatedEventHandler(IEventRepository eventRepository, IIntegrationEventPublisher integrationEventPublisher)
    {
        _eventRepository = eventRepository;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public Task Handle(ParticipantUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var participantEvent = _eventRepository.GetParticipantEvent(notification.SourceGuid, notification.ParticipantGuid);
        return _integrationEventPublisher.PublishAsync(participantEvent, cancellationToken);
    }
}
