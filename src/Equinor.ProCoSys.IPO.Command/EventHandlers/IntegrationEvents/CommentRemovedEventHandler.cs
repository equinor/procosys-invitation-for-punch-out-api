using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Command.Events;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;
namespace Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;

public class CommentRemovedEventHandler : INotificationHandler<CommentRemovedEvent>
{
    private readonly IIntegrationEventPublisher _integrationEventPublisher;

    public CommentRemovedEventHandler(IIntegrationEventPublisher integrationEventPublisher) => _integrationEventPublisher = integrationEventPublisher;

    public Task Handle(CommentRemovedEvent notification, CancellationToken cancellationToken)
    {
        var commentEvent = new CommentDeleteEvent { Plant = notification.Plant, ProCoSysGuid = notification.CommentGuid };
        return _integrationEventPublisher.PublishAsync(commentEvent, cancellationToken);
    }
}
