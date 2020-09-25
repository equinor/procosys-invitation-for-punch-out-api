using System;
using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.MeetingCommands.CreateMeeting
{
    public class CreateMeetingCommand : IRequest<Result<Guid>>
    {
        public CreateMeetingCommand(string title, string location, DateTime startDate, DateTime endDate, IEnumerable<Guid> participantOids)
        {
            Title = title;
            Location = location;
            StartDate = startDate;
            EndDate = endDate;
            ParticipantOids = participantOids;
        }

        public string Title { get; }
        public string Location { get; }
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        public IEnumerable<Guid> ParticipantOids { get; }
    }
}
