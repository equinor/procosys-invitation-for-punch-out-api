using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CancelPunchOut
{
    public class CancelPunchOutCommand : IRequest<Result<string>>, IInvitationCommandRequest
    {
        public CancelPunchOutCommand(
            int invitationId,
            string rowVersion)
        {
            InvitationId = invitationId;
            RowVersion = rowVersion;
        }

        public int InvitationId { get; }
        public string RowVersion { get; }
    }
}
