using System;
using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation
{
    public class CreateInvitationCommand : IRequest<Result<int>>
    {
        public CreateInvitationCommand(
            string title,
            string projectName,
            string type,
            CreateMeetingCommand meeting,
            IList<McPkgScopeForCommand> mcPkgScope,
            IList<CommPkgScopeForCommand> commPkgScope)
        {
            McPkgScope = mcPkgScope ?? new List<McPkgScopeForCommand>();
            CommPkgScope = commPkgScope ?? new List<CommPkgScopeForCommand>();
            Meeting = meeting;
            Title = title;
            ProjectName = projectName;
            Type = type;
        }

        public CreateMeetingCommand Meeting { get; }
        public IList<McPkgScopeForCommand> McPkgScope { get; }
        public IList<CommPkgScopeForCommand> CommPkgScope { get; }
        public string Title { get; }
        public string Type { get; }
        public string ProjectName { get; }
    }

    public class CreateMeetingCommand
    {
        public CreateMeetingCommand(
            string title,
            string bodyHtml,
            string location,
            DateTime startTime,
            DateTime endTime,
            IEnumerable<Guid> participantOids)
        {
            Title = title;
            BodyHtml = bodyHtml;
            Location = location;
            StartTime = startTime;
            EndTime = endTime;
            ParticipantOids = participantOids;
        }

        public string Title { get; }
        public string BodyHtml { get; }
        public string Location { get; }
        public DateTime StartTime { get; }
        public DateTime EndTime { get; }
        public IEnumerable<Guid> ParticipantOids { get; }
    }
}
