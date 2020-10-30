using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation
{
    public class CreateInvitationCommand : IRequest<Result<int>>
    {
        public CreateInvitationCommand(
            string title,
            string description,
            string location,
            DateTime startTime,
            DateTime endTime,
            string projectName,
            DisciplineType type,
            IList<ParticipantsForCommand> participants,
            IEnumerable<string> mcPkgScope,
            IEnumerable<string> commPkgScope)
        {
            McPkgScope = mcPkgScope != null ? mcPkgScope.ToList() : new List<string>();
            CommPkgScope = commPkgScope != null ? commPkgScope.ToList() : new List<string>();
            Participants = participants;
            Description = description;
            Location = location;
            StartTime = startTime;
            EndTime = endTime;
            ProjectName = projectName;
            Type = type;
            Title = title;
        }

        public string Description { get; }
        public string Location { get; }
        public DateTime StartTime { get; }
        public DateTime EndTime { get; }
        public IList<string> McPkgScope { get; }
        public IList<string> CommPkgScope { get; }
        public IList<ParticipantsForCommand> Participants { get; }
        public string Title { get; }
        public DisciplineType Type { get; }
        public string ProjectName { get; }
    }
}
