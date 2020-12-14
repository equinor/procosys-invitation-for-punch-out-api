using System.Collections.Generic;
using Equinor.ProCoSys.IPO.Domain;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetCommPkgsInProject
{
    public class GetCommPkgsInProjectQuery : IProjectRequest, IRequest<Result<List<ProCoSysCommPkgDto>>>
    {
        public GetCommPkgsInProjectQuery(string projectName, string startsWithCommPkgNo)
        {
            ProjectName = projectName;
            StartsWithCommPkgNo = startsWithCommPkgNo;
        }

        public string ProjectName { get; }
        public string StartsWithCommPkgNo { get; }
    }
}
