using System;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Query.GetInvitations;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationsQueries.GetInvitationsForExport
{
    public class GetInvitationsForExportQuery : IRequest<Result<ExportDto>>, IProjectRequest
    {
        public const SortingDirection DefaultSortingDirection = SortingDirection.Asc;
        public const SortingProperty DefaultSortingProperty = SortingProperty.CreatedAtUtc;

        public GetInvitationsForExportQuery(string projectName, Sorting sorting = null, Filter filter = null)
        {
            if (string.IsNullOrEmpty(projectName))
            {
                throw new ArgumentNullException(nameof(projectName));
            }
            ProjectName = projectName;
            Sorting = sorting ?? new Sorting(DefaultSortingDirection, DefaultSortingProperty);
            Filter = filter ?? new Filter();
        }

        public string ProjectName { get; }
        public Sorting Sorting { get; }
        public Filter Filter { get; }
    }
}
