using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.Common.Time;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceResult;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationsQueries.GetInvitations
{
    public class GetInvitationsQueryHandler : GetInvitationsQueryBase, IRequestHandler<GetInvitationsQuery, Result<InvitationsResult>>
    {
        private readonly IReadOnlyContext _context;
        private readonly IPermissionCache _permissionCache;
        private readonly IPlantProvider _plantProvider;
        private readonly IFunctionalRoleApiService _functionalRoleApiService;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly DateTime _utcNow;

        public GetInvitationsQueryHandler(
           IReadOnlyContext context,
           IPermissionCache permissionCache,
           IPlantProvider plantProvider,
           ICurrentUserProvider currentUserProvider,
           IFunctionalRoleApiService functionalRoleApiService)
        {
            _context = context;
            _utcNow = TimeService.UtcNow;
            _permissionCache = permissionCache;
            _plantProvider = plantProvider;
            _currentUserProvider = currentUserProvider;
            _functionalRoleApiService = functionalRoleApiService;
        }

        public async Task<Result<InvitationsResult>> Handle(GetInvitationsQuery request, CancellationToken cancellationToken)
        {
            var invitationForQueryDtos = CreateQueryableWithFilter(_context, request.ProjectName, request.Filter, _utcNow, _currentUserProvider, _permissionCache, _plantProvider);


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
            var codes = invitationsWithIncludes.SelectMany(x => x.Participants).Where(y => y.FunctionalRoleCode != null).Select(y => y.FunctionalRoleCode).Distinct().ToList();
            var functionalRoles =
                await _functionalRoleApiService.GetFunctionalRolesByCodeAsync(_plantProvider.Plant, codes);

            var functionalRolesNotUsingPersonalEmail = new List<string>();

            if (functionalRoles != null && functionalRoles.Any())
            {
                functionalRolesNotUsingPersonalEmail =
                    functionalRoles.Where(fr => fr.UsePersonalEmail != null && !fr.UsePersonalEmail.Value)
                        .Select(fr => fr.Code).ToList();
            }

            var result = CreateResult(maxAvailable, orderedInvitations, invitationsWithIncludes, functionalRolesNotUsingPersonalEmail);

            return new SuccessResult<InvitationsResult>(result);
        }

        private InvitationsResult CreateResult(
            int maxAvailable,
            IEnumerable<InvitationForQueryDto> orderedInvitations,
            List<Invitation> invitationsWithIncludes,
            List<string> functionalRolesNotUsingPersonalEmail)
        {
            var invitations = orderedInvitations.Select(invitation =>
            {
                var invitationWithIncludes = invitationsWithIncludes.Single(i => i.Id == invitation.Id);

                // Filtering out persons related to functional roles where using personal email is set to true.
                var participants = invitationWithIncludes.Participants
                    .Where(p => ((!p.HasRole) || (p.IsRole) || (p.HasRole && functionalRolesNotUsingPersonalEmail.Contains(p.FunctionalRoleCode))))
                    .ToList();

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
                    GetAdditionalContractorReps(participants),
                    GetAdditionalConstructionCompanyReps(participants),
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
