using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;
using System.Threading.Tasks;
using System.Threading;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
namespace Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;

internal class CommPkgAddedEventHandler : INotificationHandler<CommPkgAddedEvent>
{
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly ICreateEventHelper _eventHelper;

    public CommPkgAddedEventHandler(IIntegrationEventPublisher integrationEventPublisher, ICreateEventHelper eventHelper)
    {
        _integrationEventPublisher = integrationEventPublisher;
        _eventHelper = eventHelper;
    }

    public async Task Handle(CommPkgAddedEvent notification, CancellationToken cancellationToken)
    {
        var commPkgEvent = await _eventHelper.CreateCommPkgEvent(notification.CommPkg, notification.Invitation);
        await _integrationEventPublisher.PublishAsync(commPkgEvent, cancellationToken);
    }
}
