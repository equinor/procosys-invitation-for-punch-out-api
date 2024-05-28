using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;
using System.Threading.Tasks;
using System.Threading;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;

public class IpoEditedEventHandler : INotificationHandler<IpoEditedEvent>
{
    private readonly IInvitationRepository _invitationRepository;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;

    public IpoEditedEventHandler(IInvitationRepository invitationRepository, IIntegrationEventPublisher integrationEventPublisher)
    {
        _invitationRepository = invitationRepository;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public Task Handle(IpoEditedEvent notification, CancellationToken cancellationToken)
    {
        var invitationEvent = _invitationRepository.GetInvitationEvent(notification.SourceGuid);
        return _integrationEventPublisher.PublishAsync(invitationEvent, cancellationToken);
    }
}
