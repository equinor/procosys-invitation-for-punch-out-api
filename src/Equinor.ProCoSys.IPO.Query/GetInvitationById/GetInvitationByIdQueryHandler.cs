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

            var invitationResult = ConvertToInvitationDto(invitation, meeting);

            return new SuccessResult<InvitationDto>(invitationResult);
        }

        private static InvitationDto ConvertToInvitationDto(Invitation invitation, GeneralMeeting meeting)
        {
            var invitationResult = new InvitationDto(
                invitation.ProjectName,
                invitation.Title,
                invitation.Description,
                meeting.Location,
                invitation.Type)
            {
                StartTime = meeting.StartDate.DatetimeUtc,
                EndTime = meeting.EndDate.DatetimeUtc,
                Participants = ConvertToParticipantDto(invitation.Participants),
                McPkgScope = ConvertToMcPkgDto(invitation.McPkgs),
                CommPkgScope = ConvertToCommPkgDto(invitation.CommPkgs)
            };

            return invitationResult;
        }

        private static IEnumerable<CommPkgScopeDto> ConvertToCommPkgDto(IEnumerable<CommPkg> commPkgs)
            => commPkgs.Select(commPkg => new CommPkgScopeDto(commPkg.CommPkgNo, commPkg.Description, commPkg.Status));

        private static IEnumerable<McPkgScopeDto> ConvertToMcPkgDto(IEnumerable<McPkg> mcPkgs) 
            => mcPkgs.Select(mcPkg => new McPkgScopeDto(mcPkg.McPkgNo, mcPkg.Description, mcPkg.CommPkgNo));

        private static IEnumerable<ParticipantDto> ConvertToParticipantDto(IReadOnlyCollection<Participant> participants)
        {
            foreach (var participant in participants)
            {
                if (participant.Type == IpoParticipantType.FunctionalRole)
                {
                    var personsInFunctionalRole = participants
                        .Where(p => p.FunctionalRoleCode == participant.FunctionalRoleCode 
                         && p.Type == IpoParticipantType.Person);

                    yield return new ParticipantDto(participant.Organization, participant.SortKey, participant.Email,
                        null, ConvertToFunctionalRoleDto(participant, personsInFunctionalRole));
                }
                else if (ParticipantIsNotInFunctionalRole(participant))
                {
                    yield return new ParticipantDto(participant.Organization, participant.SortKey, participant.Email,
                        ConvertToPersonDto(participant), null);
                }
            }
        }

        private static bool ParticipantIsNotInFunctionalRole(Participant participant) => string.IsNullOrWhiteSpace(participant.FunctionalRoleCode);

        private static FunctionalRoleDto ConvertToFunctionalRoleDto(Participant participant, IEnumerable<Participant> personsInFunctionalRole)
            => new FunctionalRoleDto(participant.FunctionalRoleCode, participant.Email, ConvertToPersonDto(personsInFunctionalRole));

        private static PersonDto ConvertToPersonDto(Participant participant)
            => new PersonDto(participant.Id, participant.FirstName, participant.LastName, participant.AzureOid.ToString(), participant.Email);

        private static IEnumerable<PersonDto> ConvertToPersonDto(IEnumerable<Participant> personsInFunctionalRole) 
            => personsInFunctionalRole.Select(ConvertToPersonDto);
    }
}
