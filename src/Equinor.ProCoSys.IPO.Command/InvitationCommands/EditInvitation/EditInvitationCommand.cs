﻿using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.EditInvitation
{
    public class EditInvitationCommand : IRequest<Result<string>>, IInvitationCommandRequest
    {
        public EditInvitationCommand(
            int invitationId,
            string title,
            string description,
            string location,
            DateTime startTime,
            DateTime endTime,
            DisciplineType type,
            IList<ParticipantsForEditCommand> updatedParticipants,
            IEnumerable<string> updatedMcPkgScope,
            IEnumerable<string> updatedCommPkgScope,
            string rowVersion)
        {
            InvitationId = invitationId;
            UpdatedMcPkgScope = updatedMcPkgScope?.ToList() ?? new List<string>();
            UpdatedCommPkgScope = updatedCommPkgScope?.ToList() ?? new List<string>();
            UpdatedParticipants = updatedParticipants;
            Description = description;
            Location = location;
            StartTime = startTime;
            EndTime = endTime;
            Type = type;
            Title = title;
            RowVersion = rowVersion;
        }

        public int InvitationId { get; }
        public string Description { get; }
        public string Location { get; }
        public DateTime StartTime { get; }
        public DateTime EndTime { get; }
        public IList<string> UpdatedMcPkgScope { get; }
        public IList<string> UpdatedCommPkgScope { get; }
        public IList<ParticipantsForEditCommand> UpdatedParticipants { get; }
        public string Title { get; }
        public DisciplineType Type { get; }
        public string RowVersion { get; }
    }
}
