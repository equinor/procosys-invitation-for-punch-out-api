using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;

internal class ParticipantAddedEventHandler : INotificationHandler<ParticipantAddedEvent>
{
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly ICreateEventHelper _eventHelper;

    public ParticipantAddedEventHandler(IIntegrationEventPublisher integrationEventPublisher, ICreateEventHelper eventHelper)
    {
        _integrationEventPublisher = integrationEventPublisher;
        _eventHelper = eventHelper;
    }

    public async Task Handle(ParticipantAddedEvent notification, CancellationToken cancellationToken)
    {
        //Filter out participants who are members of a functional role
        if (IsPersonAddedAsMemberOfAFunctionalRole(notification))
        {
            return;
        }

        var participantEvent = await _eventHelper.CreateParticipantEvent(notification.Participant, notification.Invitation);
        await _integrationEventPublisher.PublishAsync(participantEvent, cancellationToken);
    }

    private static bool IsPersonAddedAsMemberOfAFunctionalRole(ParticipantAddedEvent notification) => 
        notification.Participant.FunctionalRoleCode is not null && notification.Participant.LastName is not null;
}
