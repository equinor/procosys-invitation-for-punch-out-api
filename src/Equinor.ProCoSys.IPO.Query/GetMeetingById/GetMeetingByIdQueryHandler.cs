using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.WebApi.Controllers.Meeting;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetMeetingById
{
    public class GetMeetingByIdQueryHandler : IRequestHandler<GetMeetingByIdQuery, Result<MeetingDto>>
    {
        private readonly IPlantProvider _plantProvider;

        public GetMeetingByIdQueryHandler(IPlantProvider plantProvider)
        {
            _plantProvider = plantProvider;
        }

        public Task<Result<MeetingDto>> Handle(GetMeetingByIdQuery request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
