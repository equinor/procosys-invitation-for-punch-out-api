using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.SignInvitation
{
    public class ParticipantsWhenSigningForCommand : IRequest<Result<Unit>>
    {
        public ParticipantsWhenSigningForCommand(
            int id,
            bool attended,
            string note)
        {
            Id = id;
            Attended = attended;
            Note = note;
        }
        public int Id { get; set; }
        public bool Attended { get; set; }
        public string Note { get; set; }
    }
}
