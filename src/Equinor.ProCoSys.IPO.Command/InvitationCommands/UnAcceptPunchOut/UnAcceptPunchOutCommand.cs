using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.UnAcceptPunchOut
{
    public class UnAcceptPunchOutCommand : IRequest<Result<string>>, IInvitationCommandRequest
    {
        public UnAcceptPunchOutCommand(
            int invitationId,
            string invitationRowVersion,
            string participantRowVersion)
        {
            InvitationId = invitationId;
            InvitationRowVersion = invitationRowVersion;
            ParticipantRowVersion = participantRowVersion;
        }

        public int InvitationId { get; }
        public string InvitationRowVersion { get; }
        public string ParticipantRowVersion { get; }
    }
}
