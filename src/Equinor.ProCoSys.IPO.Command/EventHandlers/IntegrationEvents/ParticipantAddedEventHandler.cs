using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;

public class ParticipantAddedEventHandler : INotificationHandler<ParticipantAddedEvent>
{
    private readonly IInvitationRepository _invitationRepository;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;

    public ParticipantAddedEventHandler(IInvitationRepository invitationRepository, IIntegrationEventPublisher integrationEventPublisher)
    {
        _invitationRepository = invitationRepository;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public Task Handle(ParticipantAddedEvent notification, CancellationToken cancellationToken)
    {
        var participantEvent = _invitationRepository.GetParticipantEvent(notification.SourceGuid, notification.ParticipantGuid);
        return _integrationEventPublisher.PublishAsync(participantEvent, cancellationToken);
    }
}
