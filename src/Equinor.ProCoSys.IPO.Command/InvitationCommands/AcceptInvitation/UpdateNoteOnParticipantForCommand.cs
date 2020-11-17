using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.AcceptInvitation
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
        public int Id { get; set; }
        public string Note { get; set; }
        public string RowVersion { get; set; }
    }
}
