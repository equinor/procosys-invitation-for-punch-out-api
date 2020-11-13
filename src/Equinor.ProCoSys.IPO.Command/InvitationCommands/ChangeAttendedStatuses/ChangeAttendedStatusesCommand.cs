using System.Collections.Generic;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.ChangeAttendedStatus;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.ChangeAttendedStatuses
{
    public class ChangeAttendedStatusesCommand : IRequest<Result<Unit>>, IInvitationCommandRequest
    {
        public ChangeAttendedStatusesCommand(
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
