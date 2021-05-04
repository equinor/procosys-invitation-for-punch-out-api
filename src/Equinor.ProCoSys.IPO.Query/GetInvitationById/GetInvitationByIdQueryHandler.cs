using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
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

        public GetInvitationByIdQueryHandler(
            IReadOnlyContext context,
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
                _logger.LogWarning("Fusion meeting not authorized. MeetingId={MeetingId}. ({@Exception})", invitation.MeetingId, e);
            }
            catch (Exception e)
            {
                _logger.LogError("Fusion meeting error. MeetingId={MeetingId}. ({@Exception})", invitation.MeetingId, e);
            }

            var invitationDto = ConvertToInvitationDto(invitation, meeting);

            return new SuccessResult<InvitationDto>(invitationDto);
        }

        private InvitationDto ConvertToInvitationDto(Invitation invitation,  GeneralMeeting meeting)
        {
            var canEdit = meeting != null && 
                           (meeting.Participants.Any(p => p.Person.Id == _currentUserProvider.GetCurrentUserOid()) || 
                           meeting.Organizer.Id == _currentUserProvider.GetCurrentUserOid());

            var invitationResult = new InvitationDto(
                invitation.ProjectName,
                invitation.Title,
                invitation.Description,
                invitation.Location,
                invitation.Type,
                invitation.Status,
                ConvertToPersonDto(invitation.CreatedById).Result,
                invitation.StartTimeUtc,
                invitation.EndTimeUtc,
                canEdit,
                invitation.RowVersion.ConvertToString())
            {
                Participants = ConvertToParticipantDto(invitation.Participants),
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
                        participantPersonResponse = participant.Person.Person.AzureOid == meeting.Organizer.Id
                            ? OutlookResponse.Organizer
                            : GetOutlookResponseByEmailAsync(meeting, participant.Person?.Person?.Email);
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
                            ? GetParticipantTypeByEmail(meeting, personInFunctionalRole.Person.Email)
                            : null;
                        personInFunctionalRole.Required = participantType.Equals(ParticipantType.Required);

                        var functionalRolePersonResponse = meeting != null
                            ? GetOutlookResponseByEmailAsync(meeting, personInFunctionalRole.Person.Email)
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

        private static OutlookResponse? GetOutlookResponseForFunctionalRole(IList<InvitedPersonDto> persons, OutlookResponse? frResponse)
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

        private IEnumerable<ParticipantDto> ConvertToParticipantDto(IReadOnlyCollection<Participant> participants)
        {
            var participantDtos = new List<ParticipantDto>();
            var orderedParticipants = participants.OrderBy(p => p.SortKey);
            foreach (var participant in orderedParticipants)
            {
                if (participant.Type == IpoParticipantType.FunctionalRole)
                {
                    var personsInFunctionalRole = participants
                        .Where(p => p.FunctionalRoleCode == participant.FunctionalRoleCode
                                    && p.SortKey == participant.SortKey
                                    && p.Type == IpoParticipantType.Person);

                    participantDtos.Add(new ParticipantDto(
                        participant.Organization,
                        participant.SortKey,
                        participant.SignedBy != null ? ConvertToPersonDto(participant.SignedBy).Result : null,
                        participant.SignedAtUtc,
                        participant.Note,
                        participant.Attended,
                        IsSigningParticipant(participant) && CurrentUserCanSignAsPersonInFunctionalRole(participant).Result,
                        null,
                        null,
                        ConvertToFunctionalRoleDto(participant, personsInFunctionalRole)));
                }
                else if (ParticipantIsNotInFunctionalRole(participant) && participant.Organization != Organization.External)
                {
                    participantDtos.Add(new ParticipantDto(
                        participant.Organization,
                        participant.SortKey,
                        participant.SignedBy != null ? ConvertToPersonDto(participant.SignedBy).Result : null,
                        participant.SignedAtUtc,
                        participant.Note,
                        participant.Attended,
                        IsSigningParticipant(participant) && _currentUserProvider.GetCurrentUserOid() == participant.AzureOid,
                        null,
                        ConvertToInvitedPersonDto(participant), 
                        null));
                }
                else if (participant.Organization == Organization.External)
                {
                    participantDtos.Add(new ParticipantDto(
                        participant.Organization,
                        participant.SortKey,
                        participant.SignedBy != null ? ConvertToPersonDto(participant.SignedBy).Result : null,
                        participant.SignedAtUtc,
                        participant.Note,
                        participant.Attended,
                        false,
                        new ExternalEmailDto(participant.Id, participant.Email,
                            participant.RowVersion.ConvertToString()),
                        null,
                        null));
                }
            }

            return participantDtos;
        }

        private static bool ParticipantIsNotInFunctionalRole(Participant participant) => string.IsNullOrWhiteSpace(participant.FunctionalRoleCode);

        private static FunctionalRoleDto ConvertToFunctionalRoleDto(Participant participant, IEnumerable<Participant> personsInFunctionalRole)
            => new FunctionalRoleDto(participant.FunctionalRoleCode, participant.Email, ConvertToInvitedPersonDto(personsInFunctionalRole), participant.RowVersion.ConvertToString()) {Id = participant.Id};

        private static InvitedPersonDto ConvertToInvitedPersonDto(Participant participant)
            => new InvitedPersonDto(new PersonDto(
                participant.Id,
                participant.FirstName,
                participant.LastName,
                participant.UserName,
                participant.AzureOid ?? Guid.Empty,
                participant.Email,
                participant.RowVersion.ConvertToString())); 

        private static IEnumerable<InvitedPersonDto> ConvertToInvitedPersonDto(IEnumerable<Participant> personsInFunctionalRole) 
            => personsInFunctionalRole.Select(ConvertToInvitedPersonDto).ToList();

        private bool IsSigningParticipant(Participant participant) 
            => participant.Organization != Organization.Supplier && participant.Organization != Organization.External;

        private async Task<bool> CurrentUserCanSignAsPersonInFunctionalRole(Participant participant)
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

        private Task<PersonDto> ConvertToPersonDto(int? personId) =>
            _context.QuerySet<Person>()
                .Where(p => p.Id == personId)
                .Select(p => new PersonDto(p.Id, p.FirstName, p.LastName, p.UserName, p.Oid, p.Email, p.RowVersion.ConvertToString()))
                .SingleAsync();

        private void LogFusionMeeting(GeneralMeeting meeting) //This is temporary in the start phase of IPO, to make debugging easier.
        {
            var meetingDetails = new Dictionary<string, string>
            {
                {"Meeting ID", meeting.Id.ToString()},
                {"Classification", meeting.Classification.ToString()}
            };
            var personsString = "Person, Guid, Name, Email, OutlookResponse, Type, IsOrganizer, IsResponsible";
            foreach (var p in meeting.Participants)
            {
                personsString +=
                    $" /n {p.Person}, {p.Person.Id?.ToString()}, {p.Person?.Name}, {p.Person?.Mail}, {p.OutlookResponse}, {p.Type}, {p.Organizer}, {p.Responsible}";
            }

            _logger.LogInformation("Fusion meeting ({@MeetingDetails}). ({@Persons})", meetingDetails, personsString);
        }
    }
}
