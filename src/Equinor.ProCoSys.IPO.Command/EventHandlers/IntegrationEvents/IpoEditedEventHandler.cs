using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;
using System.Threading.Tasks;
using System.Threading;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;

internal class IpoEditedEventHandler : INotificationHandler<IpoEditedEvent>
{
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly ICreateEventHelper _eventHelper;

    public IpoEditedEventHandler(IIntegrationEventPublisher integrationEventPublisher, ICreateEventHelper eventHelper)
    {
        _integrationEventPublisher = integrationEventPublisher;
        _eventHelper = eventHelper;
    }

    public async Task Handle(IpoEditedEvent notification, CancellationToken cancellationToken)
    {
        var invitationEvent = await _eventHelper.CreateInvitationEvent(notification.Invitation);
        await _integrationEventPublisher.PublishAsync(invitationEvent, cancellationToken);
    }
}
