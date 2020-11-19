using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationsByCommPkgNo
{
    public class GetInvitationsByCommPkgNoQuery : IRequest<Result<List<InvitationForMainDto>>>
    {
        public GetInvitationsByCommPkgNoQuery(string commPkgNo, string projectName)
        {
            CommPkgNo = commPkgNo;
            ProjectName = projectName;
        }

        public string CommPkgNo { get; }
        public string ProjectName { get; }
    }
}
