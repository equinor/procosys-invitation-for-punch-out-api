using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationsByCommPkgNo
{
    public class GetInvitationsByCommPkgNoQueryHandler : IRequestHandler<GetInvitationsByCommPkgNoQuery, Result<List<InvitationForMainDto>>>
    {
        private readonly IReadOnlyContext _context;

        public GetInvitationsByCommPkgNoQueryHandler(IReadOnlyContext context) 
            => _context = context;

        public async Task<Result<List<InvitationForMainDto>>> Handle(GetInvitationsByCommPkgNoQuery request,
            CancellationToken cancellationToken)
        {
            var projectFromRequest = _context.QuerySet<Project>()
                .SingleOrDefaultAsync(x => x.Name.Equals(request.ProjectName), cancellationToken); //TODO: JSOI Common query?
            
            var invitations =
                await (from i in _context.QuerySet<Invitation>()
                       from comm in _context.QuerySet<CommPkg>().Where(c => i.Id == EF.Property<int>(c, "InvitationId"))
                                       .DefaultIfEmpty()
                       from mc in _context.QuerySet<McPkg>().Where(m => i.Id == EF.Property<int>(m, "InvitationId"))
                           .DefaultIfEmpty()
                           //where i.Project.Name == request.ProjectName &&
                            where i.ProjectId == projectFromRequest.Id &&
                          (comm.CommPkgNo == request.CommPkgNo ||
                           mc.CommPkgNo == request.CommPkgNo)
                       select new InvitationForMainDto(
                                           i.Id,
                                           i.Title,
                                           i.Description,
                                           i.Type,
                                           i.Status,
                                           i.CompletedAtUtc,
                                           i.AcceptedAtUtc,
                                           i.StartTimeUtc,
                                           i.RowVersion.ConvertToString()))
                    .Distinct()
                    .ToListAsync(cancellationToken);

            return new SuccessResult<List<InvitationForMainDto>>(invitations);
        }
    }
}
