using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.ChangeAttendedStatus
{
    public class ChangeAttendedStatusCommand : IRequest<Result<Unit>>, IInvitationCommandRequest
    {
        public ChangeAttendedStatusCommand(
            int invitationId,
            string invitationRowVersion,
            IList<ParticipantToChangeAttendedStatusForCommand> participants)
        {
            InvitationId = invitationId;
            InvitationRowVersion = invitationRowVersion;
            Participants = participants;
        }

        public int InvitationId { get; set; }
        public string InvitationRowVersion { get; set; }
        public IList<ParticipantToChangeAttendedStatusForCommand> Participants { get; set; }
    }
}
