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
        var commentEvent = _invitationRepository.GetCommentEvent(notification.SourceGuid, notification.CommentGuid);
        return _integrationEventPublisher.PublishAsync(commentEvent, cancellationToken);
    }
}
