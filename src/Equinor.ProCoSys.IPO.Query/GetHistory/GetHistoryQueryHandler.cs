using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetHistory
{
    public class GetHistoryQueryHandler : IRequestHandler<GetHistoryQuery, Result<List<HistoryDto>>>
    {
        private readonly IReadOnlyContext _context;

        public GetHistoryQueryHandler(IReadOnlyContext context) => _context = context;

        public async Task<Result<List<HistoryDto>>> Handle(GetHistoryQuery request, CancellationToken cancellationToken)
        {
            var invitation = await
                (from i in _context.QuerySet<Invitation>()
                    where i.Id == request.InvitationId
                    select i).SingleOrDefaultAsync(cancellationToken);

            if (invitation == null)
            {
                return new NotFoundResult<List<HistoryDto>>(Strings.EntityNotFound(nameof(Invitation), request.InvitationId));
            }

            var invitationHistory = await (from h in _context.QuerySet<History>()
                    join i in _context.QuerySet<Invitation>() on h.ObjectGuid equals i.ObjectGuid
                    join createdBy in _context.QuerySet<Person>() on h.CreatedById equals createdBy.Id
                    where i.ObjectGuid == h.ObjectGuid
                    where i.Id == request.InvitationId
                    select new HistoryDto(
                        h.Id,
                        h.Description,
                        h.CreatedAtUtc,
                        new PersonDto(
                            createdBy.Id,
                            createdBy.FirstName,
                            createdBy.LastName,
                            createdBy.UserName,
                            createdBy.Oid,
                            createdBy.Email,
                            createdBy.RowVersion.ConvertToString()),
                        h.EventType)
                ).ToListAsync(cancellationToken);

            var invitationHistoryOrdered = invitationHistory.OrderByDescending(h => h.CreatedAtUtc).ToList();

            return new SuccessResult<List<HistoryDto>>(invitationHistoryOrdered);
        }
    }
}
