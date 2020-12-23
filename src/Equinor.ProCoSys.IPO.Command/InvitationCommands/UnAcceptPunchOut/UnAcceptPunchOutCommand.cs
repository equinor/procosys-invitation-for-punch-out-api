using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.UnAcceptPunchOut
{
    public class UnAcceptPunchOutCommand : IRequest<Result<string>>, IInvitationCommandRequest
    {
        public UnAcceptPunchOutCommand(
            int invitationId,
            Guid objectGuid,
            string invitationRowVersion,
            string participantRowVersion)
        {
            InvitationId = invitationId;
            ObjectGuid = objectGuid;
            InvitationRowVersion = invitationRowVersion;
            ParticipantRowVersion = participantRowVersion;
        }

        public int InvitationId { get; }
        public Guid ObjectGuid { get; }
        public string InvitationRowVersion { get; }
        public string ParticipantRowVersion { get; }
    }
}
