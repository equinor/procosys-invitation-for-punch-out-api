using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetPersonsInUserGroup
{
    public class GetPersonsInUserGroupQuery : IRequest<Result<List<ProCoSysPersonDto>>>
    {
        public GetPersonsInUserGroupQuery(string searchString, string userGroup)
        {
            SearchString = searchString;
            UserGroup = userGroup;
        }

        public string SearchString { get; }
        public string UserGroup { get; }
    }
}
