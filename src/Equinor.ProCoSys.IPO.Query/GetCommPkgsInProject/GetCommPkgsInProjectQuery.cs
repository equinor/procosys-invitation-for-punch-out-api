using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetCommPkgsInProject
{
    public class GetCommPkgsInProjectQuery : IRequest<Result<List<ProCoSysCommPkgDto>>>
    {
        // todo rewrite this query to take projectName instead of projectId. Inherit IProjectRequest to secure the query. Fix tests in AccessValidatorTests in #region GetCommPkgsInProjectQuery
        public GetCommPkgsInProjectQuery(int projectId, string startsWithCommPkgNo)
        {
            ProjectId = projectId;
            StartsWithCommPkgNo = startsWithCommPkgNo;
        }

        public int ProjectId { get; }
        public string StartsWithCommPkgNo { get; }
    }
}
