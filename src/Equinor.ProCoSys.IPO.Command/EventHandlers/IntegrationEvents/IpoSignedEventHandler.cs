using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;

internal class IpoSignedEventHandler : INotificationHandler<IpoSignedEvent>
{
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly ICreateEventHelper _eventHelper;

    public IpoSignedEventHandler(IIntegrationEventPublisher integrationEventPublisher, ICreateEventHelper eventHelper)
    {
        _integrationEventPublisher = integrationEventPublisher;
        _eventHelper = eventHelper;
    }

    public async Task Handle(IpoSignedEvent notification, CancellationToken cancellationToken)
    {
        var invitationEvent = await _eventHelper.CreateInvitationEvent(notification.Invitation);
        await _integrationEventPublisher.PublishAsync(invitationEvent, cancellationToken);

        var participantEvent = await _eventHelper.CreateParticipantEvent(notification.Participant, notification.Invitation);
        await _integrationEventPublisher.PublishAsync(participantEvent, cancellationToken);
    }
}
