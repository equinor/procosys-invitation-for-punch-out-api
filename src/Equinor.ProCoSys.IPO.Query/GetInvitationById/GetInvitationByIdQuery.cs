using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationById
{
    public class GetInvitationByIdQuery : IRequest<Result<InvitationDto>>, IInvitationQueryRequest
    {
        public GetInvitationByIdQuery(int invitationId) => InvitationId = invitationId;

        public int InvitationId { get; }
    }
}
