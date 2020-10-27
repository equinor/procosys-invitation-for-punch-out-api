using System;
using System.Collections.Generic;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.EditInvitation
{
    public class EditInvitationCommand : IRequest<Result<Unit>>
    {
        public EditInvitationCommand(
            int invitationId,
            string title,
            string description,
            string location,
            DateTime startTime,
            DateTime endTime,
            string projectName,
            DisciplineType type,
            IList<ParticipantsForCommand> updatedParticipants,
            IList<ParticipantsForCommand> newParticipants,
            IList<McPkgScopeForCommand> updatedMcPkgScope,
            IList<McPkgScopeForCommand> newMcPkgScope,
            IList<CommPkgScopeForCommand> updatedCommPkgScope,
            IList<CommPkgScopeForCommand> newCommPkgScope)
        {
            InvitationId = invitationId;
            UpdatedMcPkgScope = updatedMcPkgScope ?? new List<McPkgScopeForCommand>();
            UpdatedCommPkgScope = updatedCommPkgScope ?? new List<CommPkgScopeForCommand>();
            UpdatedParticipants = updatedParticipants;
            NewMcPkgScope = newMcPkgScope ?? new List<McPkgScopeForCommand>();
            NewCommPkgScope = newCommPkgScope ?? new List<CommPkgScopeForCommand>();
            NewParticipants = newParticipants ?? new List<ParticipantsForCommand>();
            Description = description;
            Location = location;
            StartTime = startTime;
            EndTime = endTime;
            ProjectName = projectName;
            Type = type;
            Title = title;
        }

        public int InvitationId { get; }
        public string Description { get; }
        public string Location { get; }
        public DateTime StartTime { get; }
        public DateTime EndTime { get; }
        public IList<McPkgScopeForCommand> UpdatedMcPkgScope { get; }
        public IList<McPkgScopeForCommand> NewMcPkgScope { get; }
        public IList<CommPkgScopeForCommand> UpdatedCommPkgScope { get; }
        public IList<CommPkgScopeForCommand> NewCommPkgScope { get; }
        public IList<ParticipantsForCommand> UpdatedParticipants { get; }
        public IList<ParticipantsForCommand> NewParticipants { get; }
        public string Title { get; }
        public DisciplineType Type { get; }
        public string ProjectName { get; }
    }
}
