using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.SignInvitation
{
    public class SignInvitationCommand : IRequest<Result<string>>, IInvitationCommandRequest
    {
        public SignInvitationCommand(
            int invitationId,
            int participantId,
            string participantRowVersion)
        {
            InvitationId = invitationId;
            ParticipantId = participantId;
            ParticipantRowVersion = participantRowVersion;
        }

        public int InvitationId { get; set; }
        public int ParticipantId { get; set; }
        public string ParticipantRowVersion { get; set; }
    }
}
