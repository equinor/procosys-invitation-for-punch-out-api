using System;
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
            CancellationToken cancellationToken)
        {
            var project = await _context.QuerySet<Project>()
                .SingleOrDefaultAsync(x => x.Name.Equals(request.ProjectName), cancellationToken);
            
            if (project is null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            var commPkgsWithMdpIpos = await (from i in _context.QuerySet<Invitation>()
                from c in _context.QuerySet<CommPkg>().Where(comm => i.Id == EF.Property<int>(comm, "InvitationId"))
                    .DefaultIfEmpty()
                     where i.ProjectId == project.Id &&
                           i.Type == DisciplineType.MDP &&
                           i.Status != IpoStatus.Canceled &&
                           request.CommPkgNos.Contains(c.CommPkgNo)
                    select new CommPkgsWithMdpIposDto(
                        c.CommPkgNo,
                        i.Id,
                        i.CreatedAtUtc,
                        i.Status == IpoStatus.Accepted))
                .Distinct()
                .ToListAsync(cancellationToken);

            var commPkgsWithLatestMdpIpoStatus = commPkgsWithMdpIpos.OrderBy(x => x.CommPkgNo).GroupBy(x => x.CommPkgNo)
                .Select(c => c.OrderByDescending(x => x.CreatedAtUtc).First()).ToList();

            return new SuccessResult<List<CommPkgsWithMdpIposDto>>(commPkgsWithLatestMdpIpoStatus);
        }
    }
}
