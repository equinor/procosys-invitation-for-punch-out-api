﻿using System.Collections.Generic;
using System.Linq;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CompleteInvitation
{
    public class CompleteInvitationCommand : IRequest<Result<string>>, IInvitationCommandRequest
    {
        public CompleteInvitationCommand(
            int invitationId,
            string invitationRowVersion,
            string participantRowVersion,
            IEnumerable<UpdateAttendedStatusAndNoteOnParticipantForCommand> participants)
        {
            InvitationId = invitationId;
            InvitationRowVersion = invitationRowVersion;
            ParticipantRowVersion = participantRowVersion;
            Participants = participants != null ? participants.ToList() : new List<UpdateAttendedStatusAndNoteOnParticipantForCommand>();
        }

        public int InvitationId { get; set; }
        public string InvitationRowVersion { get; set; }
        public string ParticipantRowVersion { get; set; }
        public IList<UpdateAttendedStatusAndNoteOnParticipantForCommand> Participants { get; set; }
    }
}
