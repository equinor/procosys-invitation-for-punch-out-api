using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetMcPkgsUnderCommPkgInProject
{
    public class GetMcPkgsUnderCommPkgInProjectQuery : IRequest<Result<List<ProCoSysMcPkgDto>>>
    {
        public GetMcPkgsUnderCommPkgInProjectQuery(string projectName, string commPkgNo)
        {
            ProjectName = projectName;
            CommPkgNo = commPkgNo;
        }

        public string ProjectName { get; }
        public string CommPkgNo { get; }
    }
}
