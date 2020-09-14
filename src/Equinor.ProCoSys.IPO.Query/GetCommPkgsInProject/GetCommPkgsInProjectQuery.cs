using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetCommPkgsInProject
{
    public class GetCommPkgsInProjectQuery : IRequest<Result<List<ProCoSysCommPkgDto>>>
    {
        public GetCommPkgsInProjectQuery(int projectId, string startsWithCommPkgNo)
        {
            ProjectId = projectId;
            StartsWithCommPkgNo = startsWithCommPkgNo;
        }

        public int ProjectId { get; }
        public string StartsWithCommPkgNo { get; }
    }
}
