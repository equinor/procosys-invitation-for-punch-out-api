using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetFunctionalRoles
{
    public class GetFunctionalRolesQuery : IRequest<Result<List<ProCoSysFunctionalRoleDto>>>
    {
        public GetFunctionalRolesQuery(string classification) => Classification = classification;

        public string Classification { get; }
    }
}
