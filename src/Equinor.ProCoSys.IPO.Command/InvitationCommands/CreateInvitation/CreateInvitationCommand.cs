using System;
using System.Collections.Generic;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation
{
    public class CreateInvitationCommand : IRequest<Result<int>>
    {
        public CreateInvitationCommand(
            string title,
            string bodyHtml,
            string location,
            DateTime startTime,
            DateTime endTime,
            string projectName,
            DisciplineType type,
            IList<ParticipantsForCommand> participants,
            IList<McPkgScopeForCommand> mcPkgScope,
            IList<CommPkgScopeForCommand> commPkgScope)
        {
            McPkgScope = mcPkgScope ?? new List<McPkgScopeForCommand>();
            CommPkgScope = commPkgScope ?? new List<CommPkgScopeForCommand>();
            Participants = participants ?? new List<ParticipantsForCommand>();
            BodyHtml = bodyHtml;
            Location = location;
            StartTime = startTime;
            EndTime = endTime;
            ProjectName = projectName;
            Type = type;
            Title = title;
        }

        public string BodyHtml { get; }
        public string Location { get; }
        public DateTime StartTime { get; }
        public DateTime EndTime { get; }
        public IList<McPkgScopeForCommand> McPkgScope { get; }
        public IList<CommPkgScopeForCommand> CommPkgScope { get; }
        public IList<ParticipantsForCommand> Participants { get; }
        public string Title { get; }
        public DisciplineType Type { get; }
        public string ProjectName { get; }
    }
}
