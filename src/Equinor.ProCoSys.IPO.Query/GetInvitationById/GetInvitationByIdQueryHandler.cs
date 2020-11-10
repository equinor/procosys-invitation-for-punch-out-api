using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Fusion.Integration.Meeting;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationById
{
    public class GetInvitationByIdQueryHandler : IRequestHandler<GetInvitationByIdQuery, Result<InvitationDto>>
    {
        private readonly IReadOnlyContext _context;
        private readonly IFusionMeetingClient _meetingClient;

        public GetInvitationByIdQueryHandler(IReadOnlyContext context, IFusionMeetingClient meetingClient)
        {
            _context = context;
            _meetingClient = meetingClient;
        }

        public async Task<Result<InvitationDto>> Handle(GetInvitationByIdQuery request, CancellationToken token)
        {
            var invitation = await _context.QuerySet<Invitation>()
                .Include(i => i.CommPkgs)
                .Include(i => i.McPkgs)
                .Include(i => i.Participants)
                .SingleOrDefaultAsync(x => x.Id == request.Id, token);

            if (invitation == null)
            {
                return new NotFoundResult<InvitationDto>(Strings.EntityNotFound(nameof(Invitation), request.Id));
            }

            var meeting = await _meetingClient.GetMeetingAsync(invitation.MeetingId, query => query.ExpandInviteBodyHtml().ExpandProperty("participants.outlookstatus"));
            if (meeting == null)
            {
                throw new Exception($"Could not get meeting with id {invitation.MeetingId} from Fusion");
            }

            var invitationDto = ConvertToInvitationDto(invitation, meeting);

            return new SuccessResult<InvitationDto>(invitationDto);
        }

        private static InvitationDto ConvertToInvitationDto(Invitation invitation, GeneralMeeting meeting)
        {
            var invitationResult = new InvitationDto(
                invitation.ProjectName,
                invitation.Title,
                invitation.Description,
                meeting.Location,
                invitation.Type,
                invitation.Status,
                invitation.RowVersion.ConvertToString())
            {
                StartTimeUtc = meeting.StartDate.DatetimeUtc,
                EndTimeUtc = meeting.EndDate.DatetimeUtc,
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
                    var participantPersonResponse = GetOutlookResponseByEmailAsync(meeting, participant.Person?.Email);
                    participant.Person.Response = participantPersonResponse;
                }

                if (participant.FunctionalRole?.Persons != null)
                {
                    foreach (var personInFunctionalRole in participant.FunctionalRole?.Persons)
                    {
                        var participantType = GetParticipantTypeByEmail(meeting, personInFunctionalRole.Email);
                        personInFunctionalRole.Required = participantType.Equals(ParticipantType.Required);

                        var functionalRolePersonResponse =
                            GetOutlookResponseByEmailAsync(meeting, personInFunctionalRole.Email);
                        personInFunctionalRole.Response = functionalRolePersonResponse;
                    }
                }

                if (participant.ExternalEmail != null)
                {
                   var externalEmailResponse = GetOutlookResponseByEmailAsync(meeting, participant.ExternalEmail?.ExternalEmail);
                    participant.ExternalEmail.Response = externalEmailResponse;
                }

                if (participant.FunctionalRole?.Email != null)
                {
                    var functionalRoleResponse = GetOutlookResponseByEmailAsync(meeting, participant.FunctionalRole.Email);
                    participant.FunctionalRole.Response = functionalRoleResponse;
                }
            }
        }

        private static OutlookResponse? GetOutlookResponseByEmailAsync(GeneralMeeting meeting, string email)
            => meeting.Participants.FirstOrDefault(p 
            => string.Equals(p.Person.Mail, email, StringComparison.CurrentCultureIgnoreCase))
            ?.OutlookResponse;

        private static ParticipantType? GetParticipantTypeByEmail(GeneralMeeting meeting, string email) 
            => meeting.Participants.FirstOrDefault(p => p.Person.Mail == email)?.Type;

        private static IEnumerable<CommPkgScopeDto> ConvertToCommPkgDto(IEnumerable<CommPkg> commPkgs)
            => commPkgs.Select(commPkg => new CommPkgScopeDto(commPkg.CommPkgNo, commPkg.Description, commPkg.Status));

        private static IEnumerable<McPkgScopeDto> ConvertToMcPkgDto(IEnumerable<McPkg> mcPkgs) 
            => mcPkgs.Select(mcPkg => new McPkgScopeDto(mcPkg.McPkgNo, mcPkg.Description, mcPkg.CommPkgNo));

        private static IEnumerable<ParticipantDto> ConvertToParticipantDto(IReadOnlyCollection<Participant> participants)
        {
            var participantDtos = new List<ParticipantDto>();

            foreach (var participant in participants)
            {
                if (participant.Type == IpoParticipantType.FunctionalRole)
                {
                    var personsInFunctionalRole = participants
                        .Where(p => p.FunctionalRoleCode == participant.FunctionalRoleCode 
                         && p.Type == IpoParticipantType.Person);

                    participantDtos.Add(new ParticipantDto(participant.Organization, participant.SortKey, null,
                        null, ConvertToFunctionalRoleDto(participant, personsInFunctionalRole)));
                }
                else if (ParticipantIsNotInFunctionalRole(participant) && participant.Organization != Organization.External)
                {
                    participantDtos.Add(new ParticipantDto(participant.Organization, participant.SortKey, null,
                        ConvertToPersonDto(participant), null));
                }
                else if (participant.Organization == Organization.External)
                {
                    participantDtos.Add(new ParticipantDto(participant.Organization, participant.SortKey,
                        new ExternalEmailDto(participant.Id, participant.Email,
                            participant.RowVersion.ConvertToString()),
                        ConvertToPersonDto(participant), null));
                }
            }

            return participantDtos;
        }

        private static bool ParticipantIsNotInFunctionalRole(Participant participant) => string.IsNullOrWhiteSpace(participant.FunctionalRoleCode);

        private static FunctionalRoleDto ConvertToFunctionalRoleDto(Participant participant, IEnumerable<Participant> personsInFunctionalRole)
            => new FunctionalRoleDto(participant.FunctionalRoleCode, participant.Email, ConvertToPersonDto(personsInFunctionalRole), participant.RowVersion.ConvertToString()) {Id = participant.Id};

        private static PersonDto ConvertToPersonDto(Participant participant)
            => new PersonDto(participant.Id, participant.FirstName, participant.LastName, participant.AzureOid.ToString(), participant.Email, participant.RowVersion.ConvertToString());

        private static IEnumerable<PersonDto> ConvertToPersonDto(IEnumerable<Participant> personsInFunctionalRole) 
            => personsInFunctionalRole.Select(ConvertToPersonDto).ToList();
    }
}
