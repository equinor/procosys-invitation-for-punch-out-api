using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationById
{
    public class GetInvitationByIdQuery : IRequest<Result<InvitationDto>>
    {
        public GetInvitationByIdQuery(int id) => Id = id;

        public int Id { get; }
    }
}
