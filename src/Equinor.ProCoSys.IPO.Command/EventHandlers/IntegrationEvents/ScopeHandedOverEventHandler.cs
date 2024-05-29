﻿using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;

public class ScopeHandedOverEventHandler : INotificationHandler<ScopeHandedOverEvent>
{
    private readonly IEventRepository _eventRepository;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;

    public ScopeHandedOverEventHandler(IEventRepository eventRepository, IIntegrationEventPublisher integrationEventPublisher)
    {
        _eventRepository = eventRepository;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public Task Handle(ScopeHandedOverEvent notification, CancellationToken cancellationToken)
    {
        var invitationEvent = _eventRepository.GetInvitationEvent(notification.SourceGuid);
        return _integrationEventPublisher.PublishAsync(invitationEvent, cancellationToken);
    }
}