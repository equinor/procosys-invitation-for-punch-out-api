using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public class UpdateAttendedStatusAndNoteOnParticipantForCommand : IRequest<Result<Unit>>
    {
        public UpdateAttendedStatusAndNoteOnParticipantForCommand(
            int id,
            bool attended,
            string note,
            string rowVersion)
        {
            Id = id;
            Attended = attended;
            Note = note;
            RowVersion = rowVersion;
        }
        public int Id { get; }
        public bool Attended { get; }
        public string Note { get; }
        public string RowVersion { get; }
    }
}
