using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;
internal class IpoAttendedStatusEventHandler : INotificationHandler<AttendedStatusUpdatedEvent>
{
    private readonly IInvitationRepository _invitationRepository;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;

    public IpoAttendedStatusEventHandler(IInvitationRepository invitationRepository, IIntegrationEventPublisher integrationEventPublisher)
    {
        _invitationRepository = invitationRepository;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public Task Handle(AttendedStatusUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var participantEvent = _invitationRepository.GetParticipantEvent(notification.SourceGuid, notification.ParticipantGuid);
        return _integrationEventPublisher.PublishAsync(participantEvent, cancellationToken);
    }
}
