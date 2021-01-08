using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetComments
{
    public class GetCommentsQuery : IRequest<Result<List<CommentDto>>>, IInvitationQueryRequest
    {
        public GetCommentsQuery(int invitationId) => InvitationId = invitationId;

        public int InvitationId { get; }
    }
}
