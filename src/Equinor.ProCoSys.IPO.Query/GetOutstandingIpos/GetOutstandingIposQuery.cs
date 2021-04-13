using Equinor.ProCoSys.IPO.Domain;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetOutstandingIpos
{
    public class GetOutstandingIposQuery : IRequest<Result<OutstandingIposResultDto>>, IProjectRequest
    {
        public GetOutstandingIposQuery(string projectName) => ProjectName = projectName;

        public string ProjectName { get; }
    }
}
