using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.UnCompletePunchOut
{
    public class UnCompletePunchOutCommand : IRequest<Result<string>>, IInvitationCommandRequest
    {
        public UnCompletePunchOutCommand(
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
