using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.Time;
using Equinor.ProCoSys.IPO.Query.GetInvitations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationsQueries.GetInvitations
{
    public class GetInvitationsQueryHandler : GetInvitationsQueryBase, IRequestHandler<GetInvitationsQuery, Result<InvitationsResult>>
    {
        private readonly IReadOnlyContext _context;

        private readonly DateTime _utcNow;

        public GetInvitationsQueryHandler(
            IReadOnlyContext context)
        {
            _context = context;
            _utcNow = TimeService.UtcNow;
        }

        public async Task<Result<InvitationsResult>> Handle(GetInvitationsQuery request, CancellationToken token)
        {
            var queryable = CreateQueryableWithFilter(_context, request.ProjectName, request.Filter, _utcNow);

            // count before adding sorting/paging
            var maxAvailable = await queryable.CountAsync(token);

            var enumerable = AddSorting(request.Sorting, queryable.AsEnumerable());
            enumerable = AddPaging(request.Paging, enumerable);

            var orderedDtos = enumerable.ToList();

            if (!orderedDtos.Any())
            {
                return new SuccessResult<InvitationsResult>(new InvitationsResult(maxAvailable, null));
            }

            return new SuccessResult<InvitationsResult>(new InvitationsResult(maxAvailable, orderedDtos));
        }

        private IEnumerable<InvitationForQueryDto> AddPaging(Paging paging, IEnumerable<InvitationForQueryDto> enumerable)
        {
            enumerable = enumerable
                .Skip(paging.Page * paging.Size)
                .Take(paging.Size);
            return enumerable;
        }
    }
}
