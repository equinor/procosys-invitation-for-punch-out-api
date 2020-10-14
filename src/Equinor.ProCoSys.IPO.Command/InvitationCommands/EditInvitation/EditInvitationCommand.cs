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
            IEnumerable<Guid> requiredParticipantOids,
            IEnumerable<string> requiredParticipantEmails,
            IEnumerable<Guid> optionalParticipantOids,
            IEnumerable<string> optionalParticipantEmails)
        {
            Title = title;
            BodyHtml = bodyHtml;
            Location = location;
            StartTime = startTime;
            EndTime = endTime;
            RequiredParticipantOids = requiredParticipantOids ?? new List<Guid>();
            RequiredParticipantEmails = requiredParticipantEmails ?? new List<string>();
            OptionalParticipantOids = optionalParticipantOids ?? new List<Guid>();
            OptionalParticipantEmails = optionalParticipantEmails ?? new List<string>();
        }

        public string Title { get; }
        public string BodyHtml { get; }
        public string Location { get; }
        public DateTime StartTime { get; }
        public DateTime EndTime { get; }
        public IEnumerable<Guid> RequiredParticipantOids { get; }
        public IEnumerable<string> RequiredParticipantEmails { get; }
        public IEnumerable<Guid> OptionalParticipantOids { get; }
        public IEnumerable<string> OptionalParticipantEmails { get; }

    }
}
