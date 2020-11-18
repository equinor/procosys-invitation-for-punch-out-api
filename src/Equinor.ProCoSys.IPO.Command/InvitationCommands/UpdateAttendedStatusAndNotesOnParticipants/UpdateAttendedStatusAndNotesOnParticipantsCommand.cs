using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.UpdateAttendedStatusAndNotesOnParticipants
{
    public class UpdateAttendedStatusAndNotesOnParticipantsCommand : IRequest<Result<Unit>>, IInvitationCommandRequest
    {
        public UpdateAttendedStatusAndNotesOnParticipantsCommand(
            int invitationId,
            IList<UpdateAttendedStatusAndNotesOnParticipantsForCommand> participants)
        {
            InvitationId = invitationId;
            Participants = participants;
        }

        public int InvitationId { get; }
        public IList<UpdateAttendedStatusAndNotesOnParticipantsForCommand> Participants { get; }
    }
}
