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
            string title,
            string bodyHtml,
            string location,
            DateTime startTime,
            DateTime endTime,
            IEnumerable<Guid> participantOids,
            IEnumerable<string> participantEmails)
        {
            Title = title;
            BodyHtml = bodyHtml;
            Location = location;
            StartTime = startTime;
            EndTime = endTime;
            ParticipantOids = participantOids ?? new List<Guid>();
            ParticipantEmails = participantEmails ?? new List<string>();
        }

        public string Title { get; }
        public string BodyHtml { get; }
        public string Location { get; }
        public DateTime StartTime { get; }
        public DateTime EndTime { get; }
        public IEnumerable<Guid> ParticipantOids { get; }
        public IEnumerable<string> ParticipantEmails { get; }
    }
}
