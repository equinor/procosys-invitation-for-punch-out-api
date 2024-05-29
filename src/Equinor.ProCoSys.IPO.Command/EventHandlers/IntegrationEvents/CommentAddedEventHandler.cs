using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;
using System.Threading.Tasks;
using System.Threading;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
namespace Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;

public class CommentAddedEventHandler : INotificationHandler<CommentAddedEvent>
{
    private readonly IEventRepository _eventRepository;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;

    public CommentAddedEventHandler(IEventRepository eventRepository, IIntegrationEventPublisher integrationEventPublisher)
    {
        _eventRepository = eventRepository;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public Task Handle(CommentAddedEvent notification, CancellationToken cancellationToken)
    {
        var commentEvent = _eventRepository.GetCommentEvent(notification.SourceGuid, notification.CommentGuid);
        return _integrationEventPublisher.PublishAsync(commentEvent, cancellationToken);
    }
}
