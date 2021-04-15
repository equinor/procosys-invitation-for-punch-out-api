using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetOutstandingIpos
{
    public class GetOutstandingIposForCurrentPersonQuery : IRequest<Result<OutstandingIposResultDto>>
    {
    }
}
