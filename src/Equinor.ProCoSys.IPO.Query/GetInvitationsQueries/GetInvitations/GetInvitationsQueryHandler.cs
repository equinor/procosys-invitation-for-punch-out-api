using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.Time;
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

        public async Task<Result<InvitationsResult>> Handle(GetInvitationsQuery request, CancellationToken cancellationToken)
        {
            var invitationForQueryDtos = CreateQueryableWithFilter(_context, request.ProjectName, request.Filter, _utcNow);

            // count before adding sorting/paging
            var maxAvailable = await invitationForQueryDtos.CountAsync(cancellationToken);

            invitationForQueryDtos = AddSorting(request.Sorting, invitationForQueryDtos);
            invitationForQueryDtos = AddPaging(request.Paging, invitationForQueryDtos);

            var orderedInvitations = await invitationForQueryDtos.ToListAsync(cancellationToken);

            if (!orderedInvitations.Any())
            {
                return new SuccessResult<InvitationsResult>(new InvitationsResult(maxAvailable, null));
            }

            var invitationIds = invitationForQueryDtos.Select(i => i.Id).ToList();

            var invitationsWithIncludes = await GetInvitationsWithIncludesAsync(_context, invitationIds, cancellationToken);

            var result = CreateResult(maxAvailable, orderedInvitations, invitationsWithIncludes);

            return new SuccessResult<InvitationsResult>(result);
        }

        private InvitationsResult CreateResult(
            int maxAvailable,
            IEnumerable<InvitationForQueryDto> orderedInvitations,
            List<Invitation> invitationsWithIncludes)
        {
            var invitations = orderedInvitations.Select(invitation =>
            {
                var invitationWithIncludes = invitationsWithIncludes.Single(i => i.Id == invitation.Id);
                var participants = invitationWithIncludes.Participants.ToList();
                return new InvitationDto(invitation.Id,
                    invitation.ProjectName,
                    invitation.Title,
                    invitation.Description,
                    invitation.Location,
                    invitation.Type,
                    invitation.Status,
                    invitation.CreatedAtUtc,
                    invitation.CreatedById,
                    invitation.StartTimeUtc,
                    invitation.EndTimeUtc,
                    invitation.CompletedAtUtc,
                    invitation.AcceptedAtUtc,
                    GetContractorRep(participants),
                    GetConstructionCompanyRep(participants),
                    GetCommissioningReps(participants),
                    GetOperationReps(participants),
                    GetTechnicalIntegrityReps(participants),
                    GetSupplierReps(participants),
                    GetExternalGuests(participants),
                    invitationWithIncludes.McPkgs.Select(mc => mc.McPkgNo),
                    invitationWithIncludes.CommPkgs.Select(c => c.CommPkgNo),
                    invitation.RowVersion);
            });
            var result = new InvitationsResult(maxAvailable, invitations);
            return result;
        }

        private IQueryable<InvitationForQueryDto> AddPaging(Paging paging, IQueryable<InvitationForQueryDto> invitationForQueryDtos)
        {
            invitationForQueryDtos = invitationForQueryDtos
                .Skip(paging.Page * paging.Size)
                .Take(paging.Size);
            return invitationForQueryDtos;
        }
    }
}
