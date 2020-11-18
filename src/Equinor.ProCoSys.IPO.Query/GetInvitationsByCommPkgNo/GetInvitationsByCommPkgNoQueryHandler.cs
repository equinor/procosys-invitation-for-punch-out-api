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

namespace Equinor.ProCoSys.IPO.Query.GetInvitationsByCommPkgNo
{
    public class GetInvitationsByCommPkgNoQueryHandler : IRequestHandler<GetInvitationsByCommPkgNoQuery, Result<List<InvitationForMainDto>>>
    {
        private readonly IReadOnlyContext _context;
        private readonly IFusionMeetingClient _meetingClient;

        public GetInvitationsByCommPkgNoQueryHandler(
            IReadOnlyContext context,
            IFusionMeetingClient meetingClient)
        {
            _context = context;
            _meetingClient = meetingClient;
        }

        public async Task<Result<List<InvitationForMainDto>>> Handle(GetInvitationsByCommPkgNoQuery request,
            CancellationToken token)
        {
            var invitations = await
                (from invitation in _context.QuerySet<Invitation>()
                    .Include(i => i.CommPkgs)
                    .Include(i => i.McPkgs)
                    .Where(i => i.ProjectName == request.ProjectName 
                                && (i.McPkgs.Any(mcPkg => mcPkg.CommPkgNo == request.CommPkgNo)
                                || i.CommPkgs.Any(commPkg => commPkg.CommPkgNo == request.CommPkgNo)))
                 select invitation).ToListAsync(token);


            var invitationForMainDtos = new List<InvitationForMainDto>();

            foreach (var invitation in invitations)
            {
                var meeting = await _meetingClient.GetMeetingAsync(invitation.MeetingId, query => query.ExpandInviteBodyHtml().ExpandProperty("participants.outlookstatus"));
                if (meeting == null)
                {
                    throw new Exception($"Could not get meeting with id {invitation.MeetingId} from Fusion");
                }

                var invitationForMainDto = ConvertToInvitationForMainDto(invitation, meeting);

                invitationForMainDtos.Add(invitationForMainDto);
            }

            return new SuccessResult<List<InvitationForMainDto>>(invitationForMainDtos);
        }

        private static InvitationForMainDto ConvertToInvitationForMainDto(Invitation invitation, GeneralMeeting meeting)
        {
            var invitationForMainDto = new InvitationForMainDto(
                    invitation.Id,
                    invitation.Title,
                    invitation.Description,
                    invitation.Type,
                    invitation.Status,
                    invitation.RowVersion.ConvertToString())
            {
                MeetingTimeUtc = meeting.StartDate.DatetimeUtc
            };

            return invitationForMainDto;
        }
    }
}
