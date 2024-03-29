﻿using Equinor.ProCoSys.Common;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationsQueries.GetInvitations
{
    public class GetInvitationsQuery : IRequest<Result<InvitationsResult>>, IProjectRequest
    {
        public const SortingDirection DefaultSortingDirection = SortingDirection.Asc;
        public const SortingProperty DefaultSortingProperty = SortingProperty.CreatedAtUtc;
        public const int DefaultPage = 0;
        public const int DefaultPagingSize = 20;

        public GetInvitationsQuery(string projectName, Sorting sorting = null, Filter filter = null, Paging paging = null)
        {
            ProjectName = projectName;
            Sorting = sorting ?? new Sorting(DefaultSortingDirection, DefaultSortingProperty);
            Filter = filter ?? new Filter();
            Paging = paging ?? new Paging(DefaultPage, DefaultPagingSize);
        }

        public string ProjectName { get; }
        public Sorting Sorting { get; }
        public Filter Filter { get; }
        public Paging Paging { get; }
    }
}
