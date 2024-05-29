using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;
using System.Threading.Tasks;
using System.Threading;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
namespace Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;

public class CommPkgAddedEventHandler : INotificationHandler<CommPkgAddedEvent>
{
    private readonly IEventRepository _eventRepository;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;

    public CommPkgAddedEventHandler(IEventRepository eventRepository, IIntegrationEventPublisher integrationEventPublisher)
    {
        _eventRepository = eventRepository;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public Task Handle(CommPkgAddedEvent notification, CancellationToken cancellationToken)
    {
        var commPkgEvent = _eventRepository.GetCommPkgEvent(notification.SourceGuid, notification.CommPkgGuid);
        return _integrationEventPublisher.PublishAsync(commPkgEvent, cancellationToken);
    }
}
