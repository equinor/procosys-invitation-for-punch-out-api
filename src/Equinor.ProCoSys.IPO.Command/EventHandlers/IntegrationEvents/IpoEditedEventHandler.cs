using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using MediatR;
using System.Threading.Tasks;
using System.Threading;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation;

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
        var invitation = _invitationRepository.GetInvitationFromLocal(notification.SourceGuid);
        var invitationEvent = _invitationRepository.GetInvitationEvent(notification.SourceGuid);
        _integrationEventPublisher.PublishAsync(invitationEvent, cancellationToken);

        //TODO: JSOI can probably be removed because of domain events in Invitation SetScope etc.
        foreach (var mcPkg in invitation.McPkgs)
        {
            var mcPkgEvent = new McPkgEvent
            {
                ProCoSysGuid = mcPkg.Guid,
                Plant = invitationEvent.Plant,
                ProjectName = invitationEvent.ProjectName,
                InvitationGuid = invitationEvent.Guid,
                CreatedAtUtc = mcPkg.CreatedAtUtc
            };

            _integrationEventPublisher.PublishAsync(mcPkgEvent, cancellationToken).GetAwaiter().GetResult();
        }

        foreach (var commPkg in invitation.CommPkgs)
        {
            var commPkgEvent = new CommPkgEvent
            {
                ProCoSysGuid = commPkg.Guid,
                Plant = invitationEvent.Plant,
                ProjectName = invitationEvent.ProjectName,
                InvitationGuid = invitationEvent.Guid,
                CreatedAtUtc = commPkg.CreatedAtUtc
            };

            _integrationEventPublisher.PublishAsync(commPkgEvent, cancellationToken).GetAwaiter().GetResult();
        }

        return Task.CompletedTask;
    }
}
