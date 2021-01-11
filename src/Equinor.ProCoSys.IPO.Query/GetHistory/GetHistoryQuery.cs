using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetHistory
{
    public class GetHistoryQuery : IRequest<Result<List<HistoryDto>>>, IInvitationQueryRequest
    {
        public GetHistoryQuery(int invitationId) => InvitationId = invitationId;

        public int InvitationId { get; }
    }
}
