using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;

public class ParticipantAddedEventHandler : INotificationHandler<ParticipantAddedEvent>
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
        //Only export functional roles and persons of type person, filter out participants who are added as members of a functional role
        if (FunctionalRoleOrPersonAsPersonType(notification))
        {
            var participantEvent = await _eventHelper.CreateParticipantEvent(notification.Participant, notification.Invitation);
            await _integrationEventPublisher.PublishAsync(participantEvent, cancellationToken);
        }
    }

    private static bool FunctionalRoleOrPersonAsPersonType(ParticipantAddedEvent notification) =>
        notification.Participant.FunctionalRoleCode is null || notification.Participant.LastName is null;
}
