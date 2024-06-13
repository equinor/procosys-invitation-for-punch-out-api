using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;

internal class ScopeHandedOverEventHandler : INotificationHandler<ScopeHandedOverEvent>
{
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly ICreateEventHelper _eventHelper;

    public ScopeHandedOverEventHandler(IIntegrationEventPublisher integrationEventPublisher, ICreateEventHelper eventHelper)
    {
        _integrationEventPublisher = integrationEventPublisher;
        _eventHelper = eventHelper;
    }

    public async Task Handle(ScopeHandedOverEvent notification, CancellationToken cancellationToken)
    {
        var invitationEvent = await _eventHelper.CreateInvitationEvent(notification.Invitation);
        await _integrationEventPublisher.PublishAsync(invitationEvent, cancellationToken);
    }
}
