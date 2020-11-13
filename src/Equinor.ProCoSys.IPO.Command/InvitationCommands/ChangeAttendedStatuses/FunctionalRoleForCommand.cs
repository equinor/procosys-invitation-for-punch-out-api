using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.ChangeAttendedStatus
{
    public class ParticipantToChangeAttendedStatusForCommand : IRequest<Result<Unit>>
    {
        public ParticipantToChangeAttendedStatusForCommand(
            int id,
            bool attended,
            string rowVersion)
        {
            Id = id;
            Attended = attended;
            RowVersion = rowVersion;
        }
        public int Id { get; set; }
        public bool Attended { get; set; }
        public string RowVersion { get; set; }
    }
}
