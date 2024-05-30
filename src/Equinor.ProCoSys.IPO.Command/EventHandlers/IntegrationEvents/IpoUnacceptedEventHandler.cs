using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;
internal class IpoUnacceptedEventHandler : INotificationHandler<IpoUnAcceptedEvent>
{
    private readonly IEventRepository _eventRepository;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;

    public IpoUnacceptedEventHandler(IEventRepository eventRepository, IIntegrationEventPublisher integrationEventPublisher)
    {
        _eventRepository = eventRepository;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public Task Handle(IpoUnAcceptedEvent notification, CancellationToken cancellationToken)
    {
        var invitationEvent = _eventRepository.GetInvitationEvent(notification.SourceGuid);
        _integrationEventPublisher.PublishAsync(invitationEvent, cancellationToken);

        var participantEvent = _eventRepository.GetParticipantEvent(notification.SourceGuid, notification.Participant.Guid);
        _integrationEventPublisher.PublishAsync(participantEvent, cancellationToken);

        return Task.CompletedTask;
    }
}
