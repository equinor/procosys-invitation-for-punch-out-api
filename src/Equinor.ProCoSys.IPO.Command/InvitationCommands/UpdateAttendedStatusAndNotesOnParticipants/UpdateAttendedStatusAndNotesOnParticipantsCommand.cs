using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.UpdateAttendedStatusAndNotesOnParticipants
{
    public class UpdateAttendedStatusAndNotesOnParticipantsCommand : IRequest<Result<Unit>>, IInvitationCommandRequest
    {
        public UpdateAttendedStatusAndNotesOnParticipantsCommand(
            int invitationId,
            string invitationRowVersion,
            IList<UpdateAttendedStatusAndNotesOnParticipantsForCommand> participants)
        {
            InvitationId = invitationId;
            InvitationRowVersion = invitationRowVersion;
            Participants = participants;
        }

        public int InvitationId { get; set; }
        public string InvitationRowVersion { get; set; }
        public IList<UpdateAttendedStatusAndNotesOnParticipantsForCommand> Participants { get; set; }
    }
}
