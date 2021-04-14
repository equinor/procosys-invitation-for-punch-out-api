using Equinor.ProCoSys.IPO.Domain;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetOutstandingIpos
{
    public class GetOutstandingIposForCurrentPersonQuery : IRequest<Result<OutstandingIposResultDto>>, IProjectRequest
    {
        public GetOutstandingIposForCurrentPersonQuery(string projectName) => ProjectName = projectName;

        public string ProjectName { get; }
    }
}
