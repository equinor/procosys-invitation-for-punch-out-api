using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;
using System.Threading.Tasks;
using System.Threading;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Command.Events;
namespace Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;

public class McPkgRemovedEventHandler : INotificationHandler<McPkgRemovedEvent>
{
    private readonly IIntegrationEventPublisher _integrationEventPublisher;

    public McPkgRemovedEventHandler(IIntegrationEventPublisher integrationEventPublisher) => _integrationEventPublisher = integrationEventPublisher;

    public Task Handle(McPkgRemovedEvent notification, CancellationToken cancellationToken)
    {
        var mcPkgRemovedEvent =
            new McPkgDeleteEvent {Plant = notification.Plant, ProCoSysGuid = notification.McPkgGuid};
        
        return _integrationEventPublisher.PublishAsync(mcPkgRemovedEvent, cancellationToken);
    }
}
