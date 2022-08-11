using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.IPO.Domain;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationsQueries.GetInvitationsForExport
{
    public class GetInvitationsForExportQuery : IRequest<Result<ExportDto>>, IProjectRequest
    {
        public const SortingDirection DefaultSortingDirection = SortingDirection.Asc;
        public const SortingProperty DefaultSortingProperty = SortingProperty.CreatedAtUtc;

        public GetInvitationsForExportQuery(List<string> projectNames, Sorting sorting = null, Filter filter = null)
        {
            ProjectName = projectNames.FirstOrDefault();
            Sorting = sorting ?? new Sorting(DefaultSortingDirection, DefaultSortingProperty);
            Filter = filter ?? new Filter();
        }

        public string ProjectName { get; }
        public Sorting Sorting { get; }
        public Filter Filter { get; }
    }
}
