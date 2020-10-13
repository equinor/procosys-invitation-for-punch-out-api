using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetPersons
{
    public class GetPersonsQuery : IRequest<Result<List<ProCoSysPersonDto>>>
    {
        public GetPersonsQuery(string searchString) => SearchString = searchString;

        public string SearchString { get; }
    }
}
