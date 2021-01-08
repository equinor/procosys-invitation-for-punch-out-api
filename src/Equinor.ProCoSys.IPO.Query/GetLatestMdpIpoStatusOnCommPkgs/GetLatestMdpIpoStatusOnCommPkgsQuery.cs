using System.Collections.Generic;
using Equinor.ProCoSys.IPO.Domain;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetLatestMdpIpoStatusOnCommPkgs
{
    public class GetLatestMdpIpoStatusOnCommPkgsQuery : IRequest<Result<List<CommPkgsWithMdpIposDto>>>, IProjectRequest
    {
        public GetLatestMdpIpoStatusOnCommPkgsQuery(IList<string> commPkgNos, string projectName)
        {
            CommPkgNos = commPkgNos;
            ProjectName = projectName;
        }

        public IList<string> CommPkgNos { get; }
        public string ProjectName { get; }
    }
}
