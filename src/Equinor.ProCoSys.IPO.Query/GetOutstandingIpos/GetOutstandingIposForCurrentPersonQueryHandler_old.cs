using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Me;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetOutstandingIpos
{
    public class GetOutstandingIposForCurrentPersonQueryHandler_old : IRequestHandler<GetOutstandingIposForCurrentPersonQuery, Result<OutstandingIposResultDto>>
    {
        private readonly IReadOnlyContext _context;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IMeApiService _meApiService;
        private readonly IPlantProvider _plantProvider;
        private readonly ILogger<GetOutstandingIposForCurrentPersonQueryHandler> _logger;

        public GetOutstandingIposForCurrentPersonQueryHandler_old(
            IReadOnlyContext context,
            ICurrentUserProvider currentUserProvider,
            IMeApiService meApiService,
            IPlantProvider plantProvider,
            ILogger<GetOutstandingIposForCurrentPersonQueryHandler> logger)
        {
            _context = context;
            _currentUserProvider = currentUserProvider;
            _meApiService = meApiService;
            _plantProvider = plantProvider;
            _logger = logger;
        }

        public async Task<Result<OutstandingIposResultDto>> Handle(GetOutstandingIposForCurrentPersonQuery request,
           CancellationToken cancellationToken)
        {
            Guid currentUserOid = default;

            try
            {
                currentUserOid = _currentUserProvider.GetCurrentUserOid();

                var invitations = await GetInvitationsAsync(currentUserOid, cancellationToken);

                var listHasFunctionalRoles =
                    invitations.Any(i =>
                        i.Participants.Any(p =>
                            p.FunctionalRoleCode != null)
                    );

                var currentUsersFunctionalRoleCodes = listHasFunctionalRoles
                    ? await _meApiService.GetFunctionalRoleCodesAsync(_plantProvider.Plant)
                    : new List<string>();

                invitations = RemoveNonRelevantFunctionalRolesInvitations(invitations, currentUserOid, currentUsersFunctionalRoleCodes);

                return new SuccessResult<OutstandingIposResultDto>(ToOutstandingIposResultDto(invitations.OrderBy(i => i.Id).ToList(), currentUserOid, currentUsersFunctionalRoleCodes));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetOustandingIPOs error for user with oid {currentUserOid}");

                return new SuccessResult<OutstandingIposResultDto>(new OutstandingIposResultDto(
                        new List<OutstandingIpoDetailsDto>()));
            }
        }

        private List<InvitationDto> RemoveNonRelevantFunctionalRolesInvitations(
            List<InvitationDto> invitations, Guid currentUserOid, IList<string> currentUsersFunctionalRoleCodes)
        {
            return invitations.Where(i =>
                i.Participants.Any(p =>
                    p.AzureOid == currentUserOid
                    && p.FunctionalRoleCode == null) 
                ||
                UserWasInvitedAsPersonInFunctionalRole(i, currentUsersFunctionalRoleCodes)).ToList();
        }

        private async Task<List<InvitationDto>> GetInvitationsAsync(
            Guid currentUserOid,
            CancellationToken cancellationToken)
        {
            var spElapsed = Stopwatch.StartNew();

            var queryable = from i in _context.QuerySet<Invitation>()
                join p in _context.QuerySet<Participant>() on i.Id equals EF.Property<int>(p, "InvitationId")
                join pro in _context.QuerySet<Project>() on i.ProjectId equals pro.Id
                where i.Status != IpoStatus.Canceled && i.Status != IpoStatus.ScopeHandedOver && pro.IsClosed == false
                      && ((!p.SignedAtUtc.HasValue
                           && p.Organization != Organization.Supplier
                           && p.Organization != Organization.External
                           && p.SortKey != 1)
                          || (p.SortKey == 1 && i.Status == IpoStatus.Completed))

                      && ((p.AzureOid == currentUserOid && p.FunctionalRoleCode == null
                          ) || (
                              p.FunctionalRoleCode != null && p.Type == IpoParticipantType.FunctionalRole))

                select new
                {
                    i.Id,
                    i.Description,
                    i.Status,
                    ParticipantId = p.Id,
                    p.AzureOid,
                    p.FunctionalRoleCode,
                    p.Organization,
                    p.SignedAtUtc,
                    p.SortKey,
                    p.Type,
                    p.SignedBy,
        };

            var filteredInvitationsFlat =
                await queryable
                    .ToListAsync(cancellationToken);

            var spElapsedms = spElapsed.ElapsedMilliseconds;

            var filteredInvitationsGrouped = filteredInvitationsFlat.GroupBy(i => new
            {
                i.Id,
                i.Description,
                i.Status
            });

            var filteredInvitationsList = new List<InvitationDto>();

            foreach (var group in filteredInvitationsGrouped)
            {
                filteredInvitationsList.Add(
                    new InvitationDto()
                    {
                        Id = group.Key.Id,
                        Description = group.Key.Description,
                        Status = group.Key.Status,
                        Participants = group.Select(p => new ParticipantDto()
                        {
                            ParticipantId = p.ParticipantId,
                            AzureOid = p.AzureOid,
                            FunctionalRoleCode = p.FunctionalRoleCode,
                            Organization = p.Organization,
                            SignedAtUtc = p.SignedAtUtc,
                            SignedBy = p.SignedBy,
                            SortKey = p.SortKey,
                            Type = p.Type
                        }).ToList()
                    });
            }

            return filteredInvitationsList;
        }

        private static bool UserWasInvitedAsPersonInFunctionalRole(InvitationDto invitation, IEnumerable<string> currentUsersFunctionalRoleCodes)
        {
            var functionalRoleParticipantCodesOnInvitation =
                invitation.Participants.Where(p =>
                                                    p.FunctionalRoleCode != null
                                                   && p.Type == IpoParticipantType.FunctionalRole)
                    .Select(p => p.FunctionalRoleCode).ToList();

            return currentUsersFunctionalRoleCodes.Any(functionalRoleCode
                => functionalRoleParticipantCodesOnInvitation.Contains(functionalRoleCode));
        }

        private OutstandingIposResultDto ToOutstandingIposResultDto(List<InvitationDto> currentUsersOutstandingInvitations,
            Guid currentUserOid, IList<string> currentUsersFunctionalRoleCodes)
        {
            var outstandingInvitations = new OutstandingIposResultDto(currentUsersOutstandingInvitations.Select(invitation =>
            {
                var organization = invitation.Participants.First(p =>
                    p.SignedBy == null &&
                    (p.AzureOid == currentUserOid ||
                     currentUsersFunctionalRoleCodes.Contains(p.FunctionalRoleCode))).Organization;
                return new OutstandingIpoDetailsDto
                {
                    InvitationId = invitation.Id,
                    Description = invitation.Description,
                    Organization = organization
                };
            }));
            return outstandingInvitations;
        }
    }
}
