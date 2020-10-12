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
        public MeetingDto(
            string title,
            string bodyHtml,
            string location,
            DateTime startTime,
            DateTime endTime,
            IEnumerable<ParticipantDto> requiredParticipants,
            IEnumerable<ParticipantDto> optionalParticipants)
        {
            Title = title;
            BodyHtml = bodyHtml;
            Location = location;
            StartTimeUtc = startTime;
            EndTimeUtc = endTime;
            RequiredParticipants = requiredParticipants;
            OptionalParticipants = optionalParticipants;
        }

        public string Title { get; }
        public string BodyHtml { get; }
        public string Location { get; }
        public DateTime StartTimeUtc { get; }
        public DateTime EndTimeUtc { get; }
        public IEnumerable<ParticipantDto> RequiredParticipants { get; }
        public IEnumerable<ParticipantDto> OptionalParticipants { get; }
    }
}
