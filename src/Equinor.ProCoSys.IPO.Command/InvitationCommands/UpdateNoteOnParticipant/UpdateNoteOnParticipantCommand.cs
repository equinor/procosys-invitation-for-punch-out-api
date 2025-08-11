using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.UpdateNoteOnParticipant
{
    public class UpdateNoteOnParticipantCommand : IRequest<Result<string>>, IInvitationCommandRequest
    {
        public UpdateNoteOnParticipantCommand(
            int invitationId,
            int participantId,
            string note,
            string rowVersion
        )
        {
            InvitationId = invitationId;
            ParticipantId = participantId;
            Note = note;
            RowVersion = rowVersion;
        }

        public int InvitationId { get; }
        public int ParticipantId { get; }
        public string Note { get; }
        public string RowVersion { get; }
    }
}
