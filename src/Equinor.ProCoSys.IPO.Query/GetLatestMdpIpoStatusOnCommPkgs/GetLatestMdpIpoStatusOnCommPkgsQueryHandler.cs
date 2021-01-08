using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetLatestMdpIpoStatusOnCommPkgs
{
    public class
        GetLatestMdpIpoStatusOnCommPkgsQueryHandler : IRequestHandler<GetLatestMdpIpoStatusOnCommPkgsQuery,
            Result<List<CommPkgsWithMdpIposDto>>>
    {
        private readonly IReadOnlyContext _context;

        public GetLatestMdpIpoStatusOnCommPkgsQueryHandler(IReadOnlyContext context)
            => _context = context;

        public async Task<Result<List<CommPkgsWithMdpIposDto>>> Handle(GetLatestMdpIpoStatusOnCommPkgsQuery request,
            CancellationToken token)
        {
            var commPkgsWithMdpIpos = await (from i in _context.QuerySet<Invitation>()
                from c in _context.QuerySet<CommPkg>().Where(comm => i.Id == EF.Property<int>(comm, "InvitationId"))
                    .DefaultIfEmpty()
                from mc in _context.QuerySet<McPkg>().Where(m => i.Id == EF.Property<int>(m, "InvitationId"))
                    .DefaultIfEmpty()
                    where i.ProjectName == request.ProjectName &&
                          i.Type == DisciplineType.MDP &&
                          (request.CommPkgNos.Contains(c.CommPkgNo) ||
                           request.CommPkgNos.Contains(mc.CommPkgNo))
                    select new CommPkgsWithMdpIposDto(
                        c.CommPkgNo ?? mc.CommPkgNo,
                        i.Id,
                        i.CreatedAtUtc,
                        i.Status == IpoStatus.Accepted))
                .Distinct()
                .ToListAsync(token);

            var commPkgsWithLatestMdpIpoStatus = commPkgsWithMdpIpos.OrderBy(x => x.CommPkgNo).GroupBy(x => x.CommPkgNo)
                .Select(c => c.OrderByDescending(x => x.CreatedAtUtc).First()).ToList();

            return new SuccessResult<List<CommPkgsWithMdpIposDto>>(commPkgsWithLatestMdpIpoStatus);
        }
    }
}
