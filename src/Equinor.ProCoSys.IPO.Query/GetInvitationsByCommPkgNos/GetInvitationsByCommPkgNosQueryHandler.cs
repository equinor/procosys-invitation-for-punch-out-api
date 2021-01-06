using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationsByCommPkgNos
{
    public class GetInvitationsByCommPkgNosQueryHandler : IRequestHandler<GetInvitationsByCommPkgNosQuery, Result<List<InvitationForMainDto>>>
    {
        private readonly IReadOnlyContext _context;

        public GetInvitationsByCommPkgNosQueryHandler(IReadOnlyContext context) 
            => _context = context;

        public async Task<Result<List<InvitationForMainDto>>> Handle(GetInvitationsByCommPkgNosQuery request,
            CancellationToken token)
        {
            var invitations = await
                (from invitation in _context.QuerySet<Invitation>()
                    .Include(i => i.CommPkgs)
                    .Include(i => i.McPkgs)
                    .Include(i => i.Participants)
                    .Where(i => i.ProjectName == request.ProjectName 
                                && (i.McPkgs.Any(mcPkg => request.CommPkgNos.Contains(mcPkg.CommPkgNo))
                                || i.CommPkgs.Any(commPkg => request.CommPkgNos.Contains(commPkg.CommPkgNo))))
                 select invitation).ToListAsync(token);

            var invitationForMainDtos = new List<InvitationForMainDto>();

            foreach (var invitation in invitations)
            {
                var invitationForMainDto = ConvertToInvitationForMainDto(invitation);
                invitationForMainDtos.Add(invitationForMainDto);
            }

            return new SuccessResult<List<InvitationForMainDto>>(invitationForMainDtos);
        }

        private static InvitationForMainDto ConvertToInvitationForMainDto(Invitation invitation)
            => new InvitationForMainDto(
                invitation.Id,
                invitation.Title,
                invitation.Type,
                invitation.Status,
                invitation.RowVersion.ConvertToString());
    }
}
