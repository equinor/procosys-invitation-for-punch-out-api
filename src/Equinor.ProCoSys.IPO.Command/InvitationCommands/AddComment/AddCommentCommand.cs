using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.AddComment
{
    public class AddCommentCommand : IRequest<Result<int>>, IInvitationCommandRequest
    {
        public AddCommentCommand(
            int invitationId,
            string comment)
        {
            InvitationId = invitationId;
            Comment = comment;
        }

        public int InvitationId { get; }
        public string Comment { get; }
    }
}
