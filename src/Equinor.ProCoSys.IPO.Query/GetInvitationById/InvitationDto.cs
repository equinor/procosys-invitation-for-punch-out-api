using System;
using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationById
{
    public class InvitationDto
    {
        public InvitationDto(MeetingDto meeting) => Meeting = meeting;

        public MeetingDto Meeting { get; }
    }

    public class MeetingDto
    {
        public MeetingDto(string title, string bodyHtml, string location, DateTime startTime, DateTime endTime, IEnumerable<Guid> participantOids)
        {
            Title = title;
            BodyHtml = bodyHtml;
            Location = location;
            StartTimeUtc = startTime;
            EndTimeUtc = endTime;
            ParticipantOids = participantOids;
        }

        public string Title { get; }
        public string BodyHtml { get; }
        public string Location { get; }
        public DateTime StartTimeUtc { get; }
        public DateTime EndTimeUtc { get; }
        public IEnumerable<Guid> ParticipantOids { get; }
    }
}
