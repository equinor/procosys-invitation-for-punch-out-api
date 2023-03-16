using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Common;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetLatestMdpIpoStatusOnCommPkgs
{
    public class GetLatestMdpIpoStatusOnCommPkgsQuery : IRequest<Result<List<CommPkgsWithMdpIposDto>>>, IProjectRequest
    {
        public GetLatestMdpIpoStatusOnCommPkgsQuery(IList<string> commPkgNos, string projectName)
        {
            CommPkgNos = commPkgNos.Where(c => !string.IsNullOrEmpty(c)).ToList();
            ProjectName = projectName;
        }

        public IList<string> CommPkgNos { get; }
        public string ProjectName { get; }
    }
}
