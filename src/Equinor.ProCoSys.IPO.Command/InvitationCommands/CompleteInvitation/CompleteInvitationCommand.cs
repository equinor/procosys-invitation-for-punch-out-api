using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CompleteInvitation
{
    public class CompleteInvitationCommand : IRequest<Result<string>>, IInvitationCommandRequest
    {
        public CompleteInvitationCommand(
            int invitationId,
            string invitationRowVersion,
            string participantRowVersion)
        {
            InvitationId = invitationId;
            InvitationRowVersion = invitationRowVersion;
            ParticipantRowVersion = participantRowVersion;
        }

        public int InvitationId { get; set; }
        public string InvitationRowVersion { get; set; }
        public string ParticipantRowVersion { get; set; }
    }
}
