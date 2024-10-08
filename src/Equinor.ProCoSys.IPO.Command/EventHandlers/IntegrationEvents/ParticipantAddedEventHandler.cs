﻿using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;

public class ParticipantAddedEventHandler : INotificationHandler<ParticipantAddedEvent>
{
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly ICreateEventHelper _eventHelper;

    public ParticipantAddedEventHandler(IIntegrationEventPublisher integrationEventPublisher, ICreateEventHelper eventHelper)
    {
        _integrationEventPublisher = integrationEventPublisher;
        _eventHelper = eventHelper;
    }

    public async Task Handle(ParticipantAddedEvent notification, CancellationToken cancellationToken)
    {
        var participantEvent = await _eventHelper.CreateParticipantEvent(notification.Participant, notification.Invitation);

        if (participantEvent is null)
        {
            return;
        }
        await _integrationEventPublisher.PublishAsync(participantEvent, cancellationToken);
    }
}
