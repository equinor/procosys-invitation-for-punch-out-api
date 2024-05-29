using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;
using System.Threading.Tasks;
using System.Threading;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
namespace Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;

public class McPkgAddedEventHandler : INotificationHandler<McPkgAddedEvent>
{
    private readonly IEventRepository _eventRepository;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;

    public McPkgAddedEventHandler(IEventRepository eventRepository, IIntegrationEventPublisher integrationEventPublisher)
    {
        _eventRepository = eventRepository;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public Task Handle(McPkgAddedEvent notification, CancellationToken cancellationToken)
    {
        var commentEvent = _eventRepository.GetMcPkgEvent(notification.SourceGuid, notification.McPkgGuid);
        return _integrationEventPublisher.PublishAsync(commentEvent, cancellationToken);
    }
}
