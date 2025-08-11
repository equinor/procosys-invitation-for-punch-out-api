using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Command.Events;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;
namespace Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;

public class CommPkgRemovedEventHandler : INotificationHandler<CommPkgRemovedEvent>
{
    private readonly IIntegrationEventPublisher _integrationEventPublisher;

    public CommPkgRemovedEventHandler(IIntegrationEventPublisher integrationEventPublisher) => _integrationEventPublisher = integrationEventPublisher;

    public Task Handle(CommPkgRemovedEvent notification, CancellationToken cancellationToken)
    {
        var commPkgRemovedEvent =
            new CommPkgDeleteEvent { Plant = notification.Plant, ProCoSysGuid = notification.CommPkgGuid };

        return _integrationEventPublisher.PublishAsync(commPkgRemovedEvent, cancellationToken);
    }
}
