using System.Collections.Generic;
using Equinor.ProCoSys.IPO.Domain;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetSavedFiltersInProject
{
    public class GetSavedFiltersInProjectQuery : IRequest<Result<List<SavedFilterDto>>> // do not care to secure this request
    {
        public GetSavedFiltersInProjectQuery(string projectName) => ProjectName = projectName;
        public string ProjectName { get; }
    }
}
