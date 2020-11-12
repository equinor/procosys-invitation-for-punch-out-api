using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.DeleteAttachment
{
    public class DeleteAttachmentCommand : IRequest<Result<Unit>>, IInvitationCommandRequest
    {
        public DeleteAttachmentCommand(int invitationId, int attachmentId, string rowVersion)
        {
            InvitationId = invitationId;
            AttachmentId = attachmentId;
            RowVersion = rowVersion;
        }

        public int InvitationId { get; }
        public int AttachmentId { get; }
        public string RowVersion { get; }
    }
}
