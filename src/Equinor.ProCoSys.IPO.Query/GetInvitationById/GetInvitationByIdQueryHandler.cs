using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Misc;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole;
using Fusion.Integration.Http.Errors;
using Fusion.Integration.Meeting;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServiceResult;
using Person = Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate.Person;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationById
{
    public class GetInvitationByIdQueryHandler : IRequestHandler<GetInvitationByIdQuery, Result<InvitationDto>>
    {
        private readonly IReadOnlyContext _context;
        private readonly IFusionMeetingClient _meetingClient;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IFunctionalRoleApiService _functionalRoleApiService;
        private readonly IPlantProvider _plantProvider;
        private readonly ILogger<GetInvitationByIdQueryHandler> _logger;

        public GetInvitationByIdQueryHandler(IReadOnlyContext context,
            IFusionMeetingClient meetingClient,
            ICurrentUserProvider currentUserProvider,
            IFunctionalRoleApiService functionalRoleApiService,
            IPlantProvider plantProvider,
            ILogger<GetInvitationByIdQueryHandler> logger)
        {
            _context = context;
            _meetingClient = meetingClient;
            _currentUserProvider = currentUserProvider;
            _functionalRoleApiService = functionalRoleApiService;
            _plantProvider = plantProvider;
            _logger = logger;
        }

        public async Task<Result<InvitationDto>> Handle(GetInvitationByIdQuery request, CancellationToken cancellationToken)
        {
            var invitation = await _context.QuerySet<Invitation>()
                .Include(i => i.CommPkgs)
                .Include(i => i.McPkgs)
                .Include(i => i.Participants)
                .SingleOrDefaultAsync(x => x.Id == request.InvitationId, cancellationToken);

            if (invitation == null)
            {
                return new NotFoundResult<InvitationDto>(Strings.EntityNotFound(nameof(Invitation), request.InvitationId));
            }

            var createdBy = await _context.QuerySet<Person>().SingleOrDefaultAsync(p => p.Id == invitation.CreatedById, cancellationToken);

            if (createdBy == null)
            {
                return new NotFoundResult<InvitationDto>(Strings.EntityNotFound(nameof(Person), invitation.CreatedById));
            }

            GeneralMeeting meeting = null;
            try
            {
                meeting = await _meetingClient.GetMeetingAsync(invitation.MeetingId,
                    query => query.ExpandInviteBodyHtml().ExpandProperty("participants.outlookstatus"));
                LogFusionMeeting(meeting);
            }
            catch (NotAuthorizedError e)
            {
                _logger.LogWarning(e, $"Fusion meeting not authorized. MeetingId={invitation.MeetingId}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Fusion meeting error. MeetingId={invitation.MeetingId}.");
            }

            var invitationDto = await ConvertToInvitationDtoAsync(invitation, meeting, createdBy);

            return new SuccessResult<InvitationDto>(invitationDto);
        }

        private async Task<InvitationDto> ConvertToInvitationDtoAsync(Invitation invitation, GeneralMeeting meeting, Person createdBy)
        {
            var canEdit = meeting != null && 
                           (meeting.Participants.Any(p => p.Person.Id == _currentUserProvider.GetCurrentUserOid()) || 
                           meeting.Organizer.Id == _currentUserProvider.GetCurrentUserOid());
            var currentUserIsCreator = _currentUserProvider.GetCurrentUserOid() == createdBy.Oid;
            var canDelete = invitation.Status == IpoStatus.Canceled && currentUserIsCreator;
            var canCancel = invitation.Status is IpoStatus.Completed or IpoStatus.Planned
                            && (currentUserIsCreator 
                                || await CurrentUserIsAmongParticipantsAsync(invitation.Participants.Where(p => p.SortKey == 0).ToList()));

            var project = await _context.QuerySet<Project>().SingleOrDefaultAsync(x => x.Id == invitation.ProjectId);
            if (project is null)
            {
                _logger.LogWarning($"Cannot find project with id {invitation.ProjectId} on invitation {invitation.Title}");
            }

            var invitationResult = new InvitationDto(
                project?.Name ?? "Project name not found",
                invitation.Title,
                invitation.Description,
                invitation.Location,
                invitation.Type,
                invitation.Status,
                ConvertToPersonDto(invitation.CreatedById).Result,
                invitation.StartTimeUtc,
                invitation.EndTimeUtc,
                canEdit,
                invitation.RowVersion.ConvertToString(), 
                canCancel,
                canDelete)
            {
                Participants = await ConvertToParticipantDtoAsync(invitation.Participants, invitation.Status),
                McPkgScope = ConvertToMcPkgDto(invitation.McPkgs),
                CommPkgScope = ConvertToCommPkgDto(invitation.CommPkgs)
            };

            AddParticipantTypeAndOutlookResponseToParticipants(meeting, invitationResult);

            return invitationResult;
        }

        private static void AddParticipantTypeAndOutlookResponseToParticipants(GeneralMeeting meeting, InvitationDto invitationResult)
        {
            foreach (var participant in invitationResult.Participants)
            {
                if (participant.Person != null)
                {
                    OutlookResponse? participantPersonResponse = null;
                    if (meeting != null)
                    {
                        participantPersonResponse = participant.Person.AzureOid == meeting.Organizer.Id
                            ? OutlookResponse.Organizer
                            : GetOutlookResponseByEmailAsync(meeting, participant.Person.Email);
                    }

                    participant.Person.Response = participantPersonResponse;
                }

                if (participant.ExternalEmail != null)
                {
                    var externalEmailResponse = meeting != null
                        ? GetOutlookResponseByEmailAsync(meeting, participant.ExternalEmail?.ExternalEmail)
                        : null;
                    participant.ExternalEmail.Response = externalEmailResponse;
                }

                if (participant.FunctionalRole != null)
                {
                    foreach (var personInFunctionalRole in participant.FunctionalRole.Persons)
                    {
                        var participantType = meeting != null
                            ? GetParticipantTypeByEmail(meeting, personInFunctionalRole.Email)
                            : null;
                        personInFunctionalRole.Required = participantType.Equals(ParticipantType.Required);

                        var functionalRolePersonResponse = meeting != null
                            ? GetOutlookResponseByEmailAsync(meeting, personInFunctionalRole.Email)
                            : null;
                        personInFunctionalRole.Response = functionalRolePersonResponse;
                    }

                    OutlookResponse? functionalRoleResponse = null;
                    if (participant.FunctionalRole.Email != null && meeting != null)
                    {
                        functionalRoleResponse =
                            GetOutlookResponseByEmailAsync(meeting, participant.FunctionalRole.Email);
                    }

                    if (participant.FunctionalRole.Persons != null && meeting != null)
                    {
                        functionalRoleResponse = GetOutlookResponseForFunctionalRole(
                            participant.FunctionalRole.Persons.ToList(),
                            functionalRoleResponse);
                    }
                    
                    participant.FunctionalRole.Response = functionalRoleResponse;
                }
            }
        }

        private static OutlookResponse? GetOutlookResponseByEmailAsync(GeneralMeeting meeting, string email)
            => meeting.Participants.FirstOrDefault(p 
            => string.Equals(p.Person.Mail, email, StringComparison.CurrentCultureIgnoreCase))
            ?.OutlookResponse;

        private static OutlookResponse? GetOutlookResponseForFunctionalRole(IList<FunctionalRolePersonDto> persons, OutlookResponse? frResponse)
        {
            if (frResponse == OutlookResponse.Accepted || persons.Any(p => p.Response == OutlookResponse.Accepted))
            {
                return OutlookResponse.Accepted;
            }
            if (frResponse == OutlookResponse.TentativelyAccepted || persons.Any(p => p.Response == OutlookResponse.TentativelyAccepted))
            {
                return OutlookResponse.TentativelyAccepted;
            }
            return persons.Any(p => p.Response == OutlookResponse.Declined) || frResponse == OutlookResponse.Declined 
                ? OutlookResponse.Declined : OutlookResponse.None;
        }

        private static ParticipantType? GetParticipantTypeByEmail(GeneralMeeting meeting, string email)
            => meeting.Participants.FirstOrDefault(p =>
                string.Equals(p.Person.Mail, email, StringComparison.CurrentCultureIgnoreCase))?.Type;

        private static IEnumerable<CommPkgScopeDto> ConvertToCommPkgDto(IEnumerable<CommPkg> commPkgs)
            => commPkgs.Select(commPkg => new CommPkgScopeDto(commPkg.CommPkgNo, commPkg.Description, commPkg.Status, commPkg.System));

        private static IEnumerable<McPkgScopeDto> ConvertToMcPkgDto(IEnumerable<McPkg> mcPkgs) 
            => mcPkgs.Select(mcPkg => new McPkgScopeDto(mcPkg.McPkgNo, mcPkg.Description, mcPkg.CommPkgNo, mcPkg.System));

        private async Task<IEnumerable<ParticipantDto>> ConvertToParticipantDtoAsync(IReadOnlyCollection<Participant> participants, IpoStatus ipoStatus)
        {
            var participantDtos = new List<ParticipantDto>();
            var orderedParticipants = participants.OrderBy(p => p.SortKey);
            var currentUserNextRequiredSigner =
                (ipoStatus == IpoStatus.Planned
                && await CurrentUserIsAmongParticipantsAsync(participants.Where(p => p.SortKey == 0 && p.SignedAtUtc == null).ToList()))
                || (ipoStatus == IpoStatus.Completed
                && await CurrentUserIsAmongParticipantsAsync(participants.Where(p => p.SortKey == 1 && p.SignedAtUtc == null).ToList()));
            foreach (var participant in orderedParticipants)
            {
                if (participant.Type == IpoParticipantType.FunctionalRole)
                {
                    var personsInFunctionalRole = participants
                        .Where(p => p.FunctionalRoleCode == participant.FunctionalRoleCode
                                    && p.SortKey == participant.SortKey
                                    && p.Type == IpoParticipantType.Person);
                    var currentUserIsInFunctionalRole = await CurrentUserIsInFunctionalRoleAsync(participant);
                    var canSign = IsSigningParticipant(participant) && currentUserIsInFunctionalRole;

                    participantDtos.Add(new ParticipantDto(
                        participant.Id,
                        participant.Organization,
                        participant.SortKey,
                        participant.SignedBy != null ? ConvertToPersonDto(participant.SignedBy).Result : null,
                        participant.SignedAtUtc,
                        participant.Note,
                        participant.Attended,
                        participant.IsAttendedTouched,
                        canSign,
                        canSign,
                        ipoStatus != IpoStatus.Canceled
                        && (currentUserNextRequiredSigner || (participant.SignedAtUtc == null && currentUserIsInFunctionalRole)),
                        null,
                        null,
                        ConvertToFunctionalRoleDto(participant, personsInFunctionalRole),
                        participant.RowVersion.ConvertToString()));
                }
                else if (ParticipantIsNotInFunctionalRole(participant) && participant.Organization != Organization.External)
                {
                    var currentUserIsParticipant = _currentUserProvider.GetCurrentUserOid() == participant.AzureOid;
                    var canSign = IsSigningParticipant(participant) && currentUserIsParticipant;
                    participantDtos.Add(new ParticipantDto(
                        participant.Id,
                        participant.Organization,
                        participant.SortKey,
                        participant.SignedBy != null ? ConvertToPersonDto(participant.SignedBy).Result : null,
                        participant.SignedAtUtc,
                        participant.Note,
                        participant.Attended,
                        participant.IsAttendedTouched,
                        canSign,
                        canSign,
                        ipoStatus != IpoStatus.Canceled
                        && (currentUserNextRequiredSigner || (participant.SignedAtUtc == null && currentUserIsParticipant)),
                        null,
                        ConvertToInvitedPersonDto(participant),
                        null,
                        participant.RowVersion.ConvertToString()));
                }
                else if (participant.Organization == Organization.External)
                {
                    participantDtos.Add(new ParticipantDto(
                        participant.Id,
                        participant.Organization,
                        participant.SortKey,
                        participant.SignedBy != null ? ConvertToPersonDto(participant.SignedBy).Result : null,
                        participant.SignedAtUtc,
                        participant.Note,
                        participant.Attended,
                        participant.IsAttendedTouched,
                        false,
                        false,
                        ipoStatus != IpoStatus.Canceled && currentUserNextRequiredSigner,
                        new ExternalEmailDto(participant.Id, participant.Email),
                        null,
                        null,
                        participant.RowVersion.ConvertToString()));
                }
            }
            return participantDtos;
        }
        private static bool ParticipantIsNotInFunctionalRole(Participant participant) => string.IsNullOrWhiteSpace(participant.FunctionalRoleCode);

        private static FunctionalRoleDto ConvertToFunctionalRoleDto(
            Participant participant,
            IEnumerable<Participant> personsInFunctionalRole)
            => new FunctionalRoleDto(
                participant.FunctionalRoleCode,
                participant.Email,
                ConvertToInvitedPersonDto(personsInFunctionalRole)) 
                {
                    Id = participant.Id
                };

        private static InvitedPersonDto ConvertToInvitedPersonDto(Participant participant)
            => new InvitedPersonDto(participant.Id,
                participant.FirstName,
                participant.LastName,
                participant.UserName,
                participant.AzureOid ?? Guid.Empty,
                participant.Email);

        private static FunctionalRolePersonDto ConvertToFunctionalRolePersonDto(Participant participant)
            => new FunctionalRolePersonDto(participant.Id,
                participant.FirstName,
                participant.LastName,
                participant.UserName,
                participant.AzureOid ?? Guid.Empty,
                participant.Email,
                participant.RowVersion.ConvertToString());

        private static IEnumerable<FunctionalRolePersonDto> ConvertToInvitedPersonDto(IEnumerable<Participant> personsInFunctionalRole) 
            => personsInFunctionalRole.Select(ConvertToFunctionalRolePersonDto).ToList();

        private bool IsSigningParticipant(Participant participant) 
            => participant.Organization != Organization.Supplier && participant.Organization != Organization.External;

        private async Task<bool> CurrentUserIsInFunctionalRoleAsync(Participant participant)
        {
            var functionalRoles = await _functionalRoleApiService.GetFunctionalRolesByCodeAsync(
                _plantProvider.Plant,
                new List<string> {participant.FunctionalRoleCode});
            var functionalRole = functionalRoles.SingleOrDefault();

            return functionalRole?.Persons != null &&
                   functionalRole.Persons.Any(person =>
                       !string.IsNullOrEmpty(person.AzureOid) &&
                       new Guid(person.AzureOid) == _currentUserProvider.GetCurrentUserOid());
        }

        private async Task<bool> CurrentUserIsAmongParticipantsAsync(IList<Participant> participants)
        {
            if (participants.Any(participant => participant.AzureOid != null && participant.AzureOid == _currentUserProvider.GetCurrentUserOid()))
            {
                return true;
            }

            foreach (var participant in participants.Where(p => p.Type == IpoParticipantType.FunctionalRole))
            {
                if (await CurrentUserIsInFunctionalRoleAsync(participant)) { return true; }
            }

            return false;
        }
        private Task<PersonDto> ConvertToPersonDto(int? personId) =>
            _context.QuerySet<Person>()
                .Where(p => p.Id == personId)
                .Select(p => new PersonDto(p.Id, p.FirstName, p.LastName, p.UserName, p.Oid, p.Email, p.RowVersion.ConvertToString()))
                .SingleAsync();

        private void LogFusionMeeting(GeneralMeeting meeting)
        {
            var message = new StringBuilder();
            message.AppendLine($"Meeting ID: {meeting.Id}");
            message.AppendLine($"Meeting Classification: {meeting.Classification}");
            foreach (var p in meeting.Participants)
            {
                message.AppendLine($" Guid:({p.Person.Id?.ToString()}), Name:({p.Person?.Name}), Email:({p.Person?.Mail}), OutlookResponse:({p.OutlookResponse}), Type:({p.Type}), IsOrganizer:({p.Organizer}), IsResponsible:({p.Responsible})");
            }

            _logger.LogInformation(message.ToString());
        }
    }
}
