using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.Time;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetInvitations
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

            queryable = AddSorting(request.Sorting, queryable);
            queryable = AddPaging(request.Paging, queryable);

            var orderedDtos = await queryable.ToListAsync(token);

            if (!orderedDtos.Any())
            {
                return new SuccessResult<InvitationsResult>(new InvitationsResult(maxAvailable, null));
            }


            return new SuccessResult<InvitationsResult>(null);
        }


        private IQueryable<InvitationForQueryDto> AddPaging(Paging paging, IQueryable<InvitationForQueryDto> queryable)
        {
            queryable = queryable
                .Skip(paging.Page * paging.Size)
                .Take(paging.Size);
            return queryable;
        }

    }
}
