using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.DeletePunchOut
{
    public class DeletePunchOutCommand : IRequest<Result<Unit>>, IInvitationCommandRequest
    {
        public DeletePunchOutCommand(
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
