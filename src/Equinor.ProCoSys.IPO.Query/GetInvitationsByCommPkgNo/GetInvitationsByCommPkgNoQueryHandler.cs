using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
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
            CancellationToken token)
        {
            var invitations = await
                (from invitation in _context.QuerySet<Invitation>()
                    .Include(i => i.CommPkgs)
                    .Include(i => i.McPkgs)
                    .Include(i => i.Participants)
                    .Where(i => i.ProjectName == request.ProjectName 
                                && (i.McPkgs.Any(mcPkg => mcPkg.CommPkgNo == request.CommPkgNo)
                                || i.CommPkgs.Any(commPkg => commPkg.CommPkgNo == request.CommPkgNo)))
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
        {
            var invitationForMainDto = new InvitationForMainDto(
                invitation.Id,
                invitation.Title,
                invitation.Description,
                invitation.Type,
                invitation.Status,
                invitation.CompletedAtUtc,
                invitation.AcceptedAtUtc,
                invitation.StartTimeUtc,
                invitation.RowVersion.ConvertToString());

            return invitationForMainDto;
        }
    }
}
