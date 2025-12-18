using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.Events;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.MessageContracts;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;

public class CreateEventHelper : ICreateEventHelper
{
    private readonly IProjectRepository _projectRepository;
    private readonly IPersonRepository _personRepository;
    private readonly ILogger<CreateEventHelper> _logger;

    public CreateEventHelper(IProjectRepository projectRepository, IPersonRepository personRepository, ILogger<CreateEventHelper> logger)
    {
        _projectRepository = projectRepository;
        _personRepository = personRepository;
        _logger = logger;
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

    [ItemCanBeNull]
    public async Task<IParticipantEventV1> CreateParticipantEvent(Participant participant, Invitation invitation)
    {
        _logger.LogInformation($"Debug: Participant: FunctionalRoleCode: {participant.FunctionalRoleCode}, LastName: {participant.LastName}, Type {participant.Type.ToString()} ");
        //Only export functional roles and persons of type person, filter out participants who are added as members of a functional role
        if (!FunctionalRoleOrPersonAsPersonType(participant))
        {
            _logger.LogInformation("Debug: NOT a functional role or person as persontype");
            return null;
        }
        _logger.LogInformation("Debug: It IS a functional role or person as persontype");

        var project = await _projectRepository.GetByIdAsync(invitation.ProjectId);

        var signedBy = participant.SignedBy.HasValue
            ? await _personRepository.GetByIdAsync(participant.SignedBy.Value)
            : null;

        return new ParticipantEvent(participant.Guid,
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

    public async Task<ICommentEventV1> CreateCommentEvent(Comment comment, Invitation invitation)
    {
        var project = await _projectRepository.GetByIdAsync(invitation.ProjectId);
        var createdBy = await _personRepository.GetByIdAsync(invitation.CreatedById);

        return new CommentEvent(comment.Guid,
            comment.CommentText,
            comment.CreatedAtUtc,
            createdBy.Guid,
            invitation.Guid,
            comment.Plant,
            project.Name);
    }

    public async Task<ICommPkgEventV1> CreateCommPkgEvent(CommPkg commPkg, Invitation invitation)
    {
        var project = await _projectRepository.GetByIdAsync(invitation.ProjectId);

        return new CommPkgEvent(
            commPkg.Guid,
            commPkg.Plant,
            project.Name,
            commPkg.Guid,
            invitation.Guid,
            commPkg.CreatedAtUtc);
    }

    public async Task<IMcPkgEventV1> CreateMcPkgEvent(McPkg mcPkg, Invitation invitation)
    {
        var project = await _projectRepository.GetByIdAsync(invitation.ProjectId);

        return new McPkgEvent(mcPkg.Guid,
            mcPkg.Plant,
            project.Name,
            mcPkg.Guid,
            invitation.Guid,
            mcPkg.CreatedAtUtc);
    }
    private static bool FunctionalRoleOrPersonAsPersonType(Participant participant) =>
        participant.FunctionalRoleCode is null || participant.LastName is null;
}
