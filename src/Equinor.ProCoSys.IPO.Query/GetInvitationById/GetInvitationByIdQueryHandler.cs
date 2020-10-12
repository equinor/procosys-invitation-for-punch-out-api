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

        public async Task<Result<InvitationDto>> Handle(GetInvitationByIdQuery request, CancellationToken cancellationToken)
        {
            var invitation = await _context.QuerySet<Invitation>().SingleOrDefaultAsync(x => x.Id == request.Id);
            if (invitation == null)
            {
                return new NotFoundResult<InvitationDto>(Strings.EntityNotFound(nameof(Invitation), request.Id));
            }

            var meeting = await _meetingClient.GetMeetingAsync(invitation.MeetingId, query => query.ExpandInviteBodyHtml().ExpandProperty("participants.outlookstatus"));
            MeetingDto meetingDto = null;
            if (meeting != null)
            {
                meetingDto = new MeetingDto(
                    meeting.Title,
                    string.Empty,
                    meeting.Location,
                    meeting.StartDate.DatetimeUtc,
                    meeting.EndDate.DatetimeUtc,
                    meeting.Participants.Where(p => p.Type == ParticipantType.Required).Select(p =>
                        new ParticipantDto(p.Person.Id, p.Person.Mail, p.Person.Name, (MeetingResponse)(p.OutlookResponse ?? OutlookResponse.Unknown))),
                    meeting.Participants.Where(p => p.Type == ParticipantType.Optional).Select(p =>
                        new ParticipantDto(p.Person.Id, p.Person.Mail, p.Person.Name, (MeetingResponse)(p.OutlookResponse ?? OutlookResponse.Unknown))));
            }

            return new SuccessResult<InvitationDto>(new InvitationDto(meetingDto));
        }
    }
}
