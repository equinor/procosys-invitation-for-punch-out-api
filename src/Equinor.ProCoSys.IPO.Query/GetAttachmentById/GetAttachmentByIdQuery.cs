using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetAttachmentById
{
    public class GetAttachmentByIdQuery : IRequest<Result<Uri>>
    {
        public GetAttachmentByIdQuery(int invitationId, int attachmentId)
        {
            InvitationId = invitationId;
            AttachmentId = attachmentId;
        }

        public int InvitationId { get; }
        public int AttachmentId { get; }
    }
}
