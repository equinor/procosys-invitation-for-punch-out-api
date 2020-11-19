using System.Collections.Generic;
using System.Linq;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.AcceptPunchOut
{
    public class AcceptPunchOutCommand : IRequest<Result<string>>, IInvitationCommandRequest
    {
        public AcceptPunchOutCommand(
            int invitationId,
            string invitationRowVersion,
            string participantRowVersion,
            IEnumerable<UpdateNoteOnParticipantForCommand> participants)
        {
            InvitationId = invitationId;
            InvitationRowVersion = invitationRowVersion;
            ParticipantRowVersion = participantRowVersion;
            Participants = participants != null ? participants.ToList() : new List<UpdateNoteOnParticipantForCommand>();
        }

        public int InvitationId { get; set; }
        public string InvitationRowVersion { get; set; }
        public string ParticipantRowVersion { get; set; }
        public IList<UpdateNoteOnParticipantForCommand> Participants { get; set; }
    }
}
