using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetAttachments
{
    public class GetAttachmentsQuery : IRequest<Result<List<AttachmentDto>>>
    {
        public GetAttachmentsQuery(int tagId) => InvitationId = tagId;

        public int InvitationId { get; }
    }
}
