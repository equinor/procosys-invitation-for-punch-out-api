using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.EditParticipants
{
    public class EditParticipantsCommand : IRequest<Result<Unit>>, IInvitationCommandRequest
    {
        public EditParticipantsCommand(
            int invitationId,
            IList<ParticipantsForEditCommand> updatedParticipants)
        {
            InvitationId = invitationId;
            UpdatedParticipants = updatedParticipants;
        }

        public int InvitationId { get; }
        public IList<ParticipantsForEditCommand> UpdatedParticipants { get; }
    }
}
