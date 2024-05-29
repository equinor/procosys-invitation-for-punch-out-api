using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;
using System.Threading.Tasks;
using System.Threading;
using Equinor.ProCoSys.IPO.Command.Events;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;

public class IpoEditedEventHandler : INotificationHandler<IpoEditedEvent>
{
    private readonly IEventRepository _eventRepository;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;

    public IpoEditedEventHandler(IEventRepository eventRepository, IIntegrationEventPublisher integrationEventPublisher)
    {
        _eventRepository = eventRepository;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public Task Handle(IpoEditedEvent notification, CancellationToken cancellationToken)
    {
        var invitationEvent = _eventRepository.GetInvitationEvent(notification.SourceGuid);
        _integrationEventPublisher.PublishAsync(invitationEvent, cancellationToken);

        return Task.CompletedTask;
    }
}
