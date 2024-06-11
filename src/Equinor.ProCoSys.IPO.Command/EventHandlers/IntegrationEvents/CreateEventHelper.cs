using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.Events;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.MessageContracts;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;
internal class CreateEventHelper : ICreateEventHelper
{
    private readonly IProjectRepository _projectRepository;
    private readonly IPersonRepository _personRepository;

    public CreateEventHelper(IProjectRepository projectRepository, IPersonRepository personRepository)
    {
        _projectRepository = projectRepository;
        _personRepository = personRepository;
    }

    public async Task<IInvitationEventV1> CreateInvitationEvent(Invitation invitation)
    {
        var project = await _projectRepository.GetByIdAsync(invitation.ProjectId);
        var createdBy = await _personRepository.GetByIdAsync(invitation.CreatedById);
        var completedBy = invitation.CompletedBy.HasValue
            ? await _personRepository.GetByIdAsync(invitation.CompletedBy.Value)
            : null;

        var acceptedBy = invitation.AcceptedBy.HasValue
            ? await _personRepository.GetByIdAsync(invitation.AcceptedBy.Value)
            : null;

        return new InvitationEvent(
            invitation.Guid,
            invitation.Guid,
            invitation.Plant,
            project.Name,
            invitation.Id,
            invitation.CreatedAtUtc,
            createdBy.Guid,
            invitation.ModifiedAtUtc,
            invitation.Title,
            invitation.Type.ToString(),
            invitation.Description,
            invitation.Status.ToString(),
            invitation.EndTimeUtc,
            invitation.Location,
            invitation.StartTimeUtc,
            invitation.AcceptedAtUtc,
        acceptedBy?.Guid,
        invitation.CompletedAtUtc,
            completedBy?.Guid);
    }

    public async Task<IParticipantEventV1> CreateParticipantEvent(Participant participant, Invitation invitation)
    {
        var project = await _projectRepository.GetByIdAsync(invitation.ProjectId);

        var signedBy = participant.SignedBy.HasValue
            ? await _personRepository.GetByIdAsync(participant.SignedBy.Value)
            : null;

        return new ParticipantEvent(invitation.Guid,
            invitation.Guid,
            invitation.Plant,
            project.Name,
            participant.Organization.ToString(),
            participant.Type.ToString(),
            participant.FunctionalRoleCode,
            participant.AzureOid,
            participant.SortKey,
            participant.CreatedAtUtc,
            invitation.Guid,
            participant.ModifiedAtUtc,
            participant.Attended,
            participant.Note,
            participant.SignedAtUtc,
            signedBy?.Guid);
    }
}
