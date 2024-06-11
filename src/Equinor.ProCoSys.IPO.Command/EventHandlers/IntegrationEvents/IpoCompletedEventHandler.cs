using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Command.Events;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using Equinor.ProCoSys.IPO.MessageContracts;
using MediatR;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;
internal class IpoCompletedEventHandler : INotificationHandler<IpoCompletedEvent>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;

    public IpoCompletedEventHandler(IProjectRepository projectRepository, IPersonRepository personRepository, IEventRepository eventRepository, IIntegrationEventPublisher integrationEventPublisher)
    {
        _projectRepository = projectRepository;
        _personRepository = personRepository;
        _eventRepository = eventRepository;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public async Task Handle(IpoCompletedEvent notification, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(notification.Invitation.ProjectId);
        var createdBy = await _personRepository.GetByIdAsync(notification.Invitation.CreatedById);

        var completedBy = notification.Invitation.CompletedBy.HasValue
            ? await _personRepository.GetByIdAsync(notification.Invitation.CompletedBy.Value)
            : null;

        var acceptedBy = notification.Invitation.AcceptedBy.HasValue
            ? await _personRepository.GetByIdAsync(notification.Invitation.AcceptedBy.Value)
            : null;
    
        var invitationEvent = CreateInvitationEvent(notification, project, completedBy, acceptedBy, createdBy);
        await _integrationEventPublisher.PublishAsync(invitationEvent, cancellationToken);

        var participantEvent = _eventRepository.GetParticipantEvent(notification.SourceGuid, notification.Participant.Guid);
        await _integrationEventPublisher.PublishAsync(participantEvent, cancellationToken);
    }

    private IInvitationEventV1 CreateInvitationEvent(IpoCompletedEvent notification, Project project, Person completedBy, Person acceptedBy, Person createdBy) =>
        new InvitationEvent(notification.SourceGuid,
            notification.SourceGuid,
            notification.Plant,
            project.Name,
            notification.Invitation.Id,
            notification.Invitation.CreatedAtUtc,
            createdBy.Guid,
            notification.Invitation.ModifiedAtUtc,
            notification.Invitation.Title,
            notification.Invitation.Type.ToString(),
            notification.Invitation.Description,
            notification.Invitation.Status.ToString(),
            notification.Invitation.EndTimeUtc,
            notification.Invitation.Location,
            notification.Invitation.StartTimeUtc,
            notification.Invitation.AcceptedAtUtc,
            acceptedBy?.Guid,
            notification.Invitation.CompletedAtUtc,
            completedBy?.Guid);
}
