using System;
using Equinor.ProCoSys.IPO.WebApi.Controllers.Meeting;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetMeetingById
{
    public class GetMeetingByIdQuery : IRequest<Result<MeetingDto>>
    {
        public GetMeetingByIdQuery(Guid meetingId)
        {
            MeetingId = meetingId;
        }

        public Guid MeetingId { get; }
    }
}
