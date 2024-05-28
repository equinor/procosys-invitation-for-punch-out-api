﻿using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;
using System.Threading.Tasks;
using System.Threading;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;

public class NoteUpdatedEventHandler : INotificationHandler<NoteUpdatedEvent>
{
    private readonly IInvitationRepository _invitationRepository;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;

    public NoteUpdatedEventHandler(IInvitationRepository invitationRepository, IIntegrationEventPublisher integrationEventPublisher)
    {
        _invitationRepository = invitationRepository;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public Task Handle(NoteUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var participantEvent = _invitationRepository.GetParticipantEvent(notification.SourceGuid, notification.ParticipantGuid);
        return _integrationEventPublisher.PublishAsync(participantEvent, cancellationToken);
    }
}
