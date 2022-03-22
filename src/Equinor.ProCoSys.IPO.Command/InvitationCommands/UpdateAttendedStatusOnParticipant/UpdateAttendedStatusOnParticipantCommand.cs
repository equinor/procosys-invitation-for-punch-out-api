using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.UpdateAttendedStatusOnParticipant
{
    public class UpdateAttendedStatusOnParticipantCommand : IRequest<Result<string>>, IInvitationCommandRequest
    {
        public UpdateAttendedStatusOnParticipantCommand(
            int invitationId,
            int participantId,
            bool attended, 
            string rowVersion
        )
        {
            InvitationId = invitationId;
            ParticipantId = participantId;
            Attended = attended;
            RowVersion = rowVersion;
        }

        public int InvitationId { get; }
        public int ParticipantId { get; }
        public bool Attended { get; }
        public string RowVersion { get; }
    }
}
