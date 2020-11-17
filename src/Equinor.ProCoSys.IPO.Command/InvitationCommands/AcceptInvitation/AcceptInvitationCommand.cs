using System.Collections.Generic;
using System.Linq;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.AcceptInvitation
{
    public class AcceptInvitationCommand : IRequest<Result<string>>, IInvitationCommandRequest
    {
        public AcceptInvitationCommand(
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
