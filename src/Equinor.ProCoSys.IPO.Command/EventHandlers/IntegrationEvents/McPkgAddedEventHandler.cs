using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;

internal class McPkgAddedEventHandler : INotificationHandler<McPkgAddedEvent>
{
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly ICreateEventHelper _eventHelper;

    public McPkgAddedEventHandler(IIntegrationEventPublisher integrationEventPublisher, ICreateEventHelper eventHelper)
    {
        _integrationEventPublisher = integrationEventPublisher;
        _eventHelper = eventHelper;
    }

    public async Task Handle(McPkgAddedEvent notification, CancellationToken cancellationToken)
    {
        var mcPkgEvent = await _eventHelper.CreateMcPkgEvent(notification.McPkg, notification.Invitation);
        await _integrationEventPublisher.PublishAsync(mcPkgEvent, cancellationToken);
    }
}
