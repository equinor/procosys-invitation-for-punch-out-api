using System.Collections.Generic;
using Equinor.ProCoSys.IPO.Domain;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationsByCommPkgNos
{
    public class GetInvitationsByCommPkgNosQuery : IRequest<Result<List<InvitationForMainDto>>>, IProjectRequest
    {
        public GetInvitationsByCommPkgNosQuery(IList<string> commPkgNos, string projectName)
        {
            CommPkgNos = commPkgNos;
            ProjectName = projectName;
        }

        public IList<string> CommPkgNos { get; }
        public string ProjectName { get; }
    }
}
