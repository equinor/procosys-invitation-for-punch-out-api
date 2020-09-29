using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetFunctionalRoles
{
    public class GetFunctionalRolesForIpoQuery : IRequest<Result<List<ProCoSysFunctionalRoleDto>>>
    {
        public GetFunctionalRolesForIpoQuery(string classification) => Classification = classification;

        public string Classification { get; }
    }
}
