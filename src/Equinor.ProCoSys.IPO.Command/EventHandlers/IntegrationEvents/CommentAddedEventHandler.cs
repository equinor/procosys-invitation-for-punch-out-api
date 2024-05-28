using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;
using System.Threading.Tasks;
using System.Threading;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
namespace Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;

public class CommentAddedEventHandler : INotificationHandler<CommentAddedEvent>
{
    private readonly IInvitationRepository _invitationRepository;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;

    public CommentAddedEventHandler(IInvitationRepository invitationRepository, IIntegrationEventPublisher integrationEventPublisher)
    {
        _invitationRepository = invitationRepository;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public Task Handle(CommentAddedEvent notification, CancellationToken cancellationToken)
    {
        //const EventType eventType = EventType.CommentAdded;
        //var history = new History(notification.Plant, eventType.GetDescription(), notification.SourceGuid, eventType);
        //_invitationRepository.Add(history);
        //return Task.CompletedTask;

        var commentEvent = _invitationRepository.GetCommentEvent(notification.SourceGuid, notification.CommentGuid);
        return _integrationEventPublisher.PublishAsync(commentEvent, cancellationToken);
    }
}
