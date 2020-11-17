using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.UpdateAttendedStatusAndNotesOnParticipants
{
    public class UpdateAttendedStatusAndNotesOnParticipantsCommand : IRequest<Result<Unit>>, IInvitationCommandRequest
    {
        public UpdateAttendedStatusAndNotesOnParticipantsCommand(
            int invitationId,
            IList<UpdateAttendedStatusAndNoteOnParticipantForCommand> participants)
        {
            InvitationId = invitationId;
            Participants = participants;
        }

        public int InvitationId { get; set; }
        public IList<UpdateAttendedStatusAndNoteOnParticipantForCommand> Participants { get; set; }
    }
}
