using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;
using System.Threading.Tasks;
using System.Threading;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
namespace Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;

internal class CommentAddedEventHandler : INotificationHandler<CommentAddedEvent>
{
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly ICreateEventHelper _eventHelper;

    public CommentAddedEventHandler(IIntegrationEventPublisher integrationEventPublisher, ICreateEventHelper eventHelper)
    {
        _integrationEventPublisher = integrationEventPublisher;
        _eventHelper = eventHelper;
    }

    public async Task Handle(CommentAddedEvent notification, CancellationToken cancellationToken)
    {
        var commentEvent = await _eventHelper.CreateCommentEvent(notification.Comment, notification.Invitation);
        await _integrationEventPublisher.PublishAsync(commentEvent, cancellationToken);
    }
}
