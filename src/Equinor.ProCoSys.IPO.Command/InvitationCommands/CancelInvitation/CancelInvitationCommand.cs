using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CancelInvitation
{
    public class CancelInvitationCommand : IRequest<Result<object>>, IInvitationCommandRequest
    {
        public CancelInvitationCommand(int invitationId) => InvitationId = invitationId;

        public int InvitationId { get; init; }
    }
}
