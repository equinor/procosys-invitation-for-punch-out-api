using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetMcPkgsInProject
{
    public class GetMcPkgsInProjectQuery : IRequest<Result<List<ProCoSysMcPkgDto>>>
    {
        public GetMcPkgsInProjectQuery(int projectId, string startsWithMcPkgNo)
        {
            ProjectId = projectId;
            StartsWithMcPkgNo = startsWithMcPkgNo;
        }

        public int ProjectId { get; }
        public string StartsWithMcPkgNo { get; }
    }
}
