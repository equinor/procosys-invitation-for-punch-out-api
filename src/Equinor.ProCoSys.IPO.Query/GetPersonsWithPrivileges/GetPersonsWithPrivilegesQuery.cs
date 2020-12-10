using System.Collections.Generic;
using Equinor.ProCoSys.IPO.Query.GetPersons;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetPersonsWithPrivileges
{
    public class GetPersonsWithPrivilegesQuery : IRequest<Result<List<ProCoSysPersonDto>>>
    {
        public GetPersonsWithPrivilegesQuery(string searchString, string objectName, IList<string> privileges)
        {
            SearchString = searchString;
            ObjectName = objectName;
            Privileges = privileges;
        }

        public string SearchString { get; }
        public string ObjectName { get; }
        public IList<string> Privileges { get; }
    }
}
