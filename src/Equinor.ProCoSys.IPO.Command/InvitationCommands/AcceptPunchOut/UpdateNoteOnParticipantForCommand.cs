using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.AcceptPunchOut
{
    public class UpdateNoteOnParticipantForCommand : IRequest<Result<Unit>>
    {
        public UpdateNoteOnParticipantForCommand(
            int id,
            string note,
            string rowVersion)
        {
            Id = id;
            Note = note;
            RowVersion = rowVersion;
        }
        public int Id { get; }
        public string Note { get; }
        public string RowVersion { get; }
    }
}
