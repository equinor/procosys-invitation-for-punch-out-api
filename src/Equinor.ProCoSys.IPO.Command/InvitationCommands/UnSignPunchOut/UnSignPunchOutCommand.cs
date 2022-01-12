using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.UnSignPunchOut
{
    public class UnSignPunchOutCommand : IRequest<Result<string>>, IInvitationCommandRequest
    {
        public UnSignPunchOutCommand(
            int invitationId,
            int participantId,
            string participantRowVersion)
        {
            InvitationId = invitationId;
            ParticipantId = participantId;
            ParticipantRowVersion = participantRowVersion;
        }

        public int InvitationId { get; }
        public int ParticipantId { get; }
        public string ParticipantRowVersion { get; }
    }
}
