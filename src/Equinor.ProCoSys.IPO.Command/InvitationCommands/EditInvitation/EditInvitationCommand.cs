using System;
using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.EditInvitation
{
    public class EditInvitationCommand : IRequest<Result<Unit>>
    {
        public EditInvitationCommand(int invitationId, EditMeetingCommand meeting)
        {
            InvitationId = invitationId;
            Meeting = meeting;
        }

        public int InvitationId { get; }
        public EditMeetingCommand Meeting { get; }
    }

    public class EditMeetingCommand
    {
        public EditMeetingCommand(
            string meetingTitle,
            string meetingBodyHtml,
            string meetingLocation,
            DateTime meetingStartTime,
            DateTime meetingEndTime,
            IEnumerable<Guid> meetingParticipantOids)
        {
            Title = meetingTitle;
            BodyHtml = meetingBodyHtml;
            Location = meetingLocation;
            StartTime = meetingStartTime;
            EndTime = meetingEndTime;
            ParticipantOids = meetingParticipantOids;
        }

        public string Title { get; }
        public string BodyHtml { get; }
        public string Location { get; }
        public DateTime StartTime { get; }
        public DateTime EndTime { get; }
        public IEnumerable<Guid> ParticipantOids { get; }
    }
}
