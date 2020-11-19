﻿using System.Collections.Generic;
using System.Linq;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CompletePunchOut
{
    public class CompletePunchOutCommand : IRequest<Result<string>>, IInvitationCommandRequest
    {
        public CompletePunchOutCommand(
            int invitationId,
            string invitationRowVersion,
            string participantRowVersion,
            IEnumerable<UpdateAttendedStatusAndNotesOnParticipantsForCommand> participants)
        {
            InvitationId = invitationId;
            InvitationRowVersion = invitationRowVersion;
            ParticipantRowVersion = participantRowVersion;
            Participants = participants != null ? participants.ToList() : new List<UpdateAttendedStatusAndNotesOnParticipantsForCommand>();
        }

        public int InvitationId { get; }
        public string InvitationRowVersion { get; }
        public string ParticipantRowVersion { get; }
        public IList<UpdateAttendedStatusAndNotesOnParticipantsForCommand> Participants { get; }
    }
}
