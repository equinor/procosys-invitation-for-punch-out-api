using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;
using System.Threading.Tasks;
using System.Threading;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;

public class StatusResetEventHandler : INotificationHandler<StatusResetEvent>
{
    private readonly IEventRepository _eventRepository;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;

    public StatusResetEventHandler(IEventRepository eventRepository, IIntegrationEventPublisher integrationEventPublisher)
    {
        _eventRepository = eventRepository;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public Task Handle(StatusResetEvent notification, CancellationToken cancellationToken)
    {
        var invitationEvent = _eventRepository.GetInvitationEvent(notification.SourceGuid);
        return _integrationEventPublisher.PublishAsync(invitationEvent, cancellationToken);
    }
}
