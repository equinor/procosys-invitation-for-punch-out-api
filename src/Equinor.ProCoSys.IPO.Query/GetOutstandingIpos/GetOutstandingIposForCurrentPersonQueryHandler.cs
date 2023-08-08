using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Me;
using Equinor.ProCoSys.IPO.Infrastructure.Repositories.RawSql.OutstandingIPOs;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetOutstandingIpos
{
    public class GetOutstandingIposForCurrentPersonQueryHandler : IRequestHandler<GetOutstandingIposForCurrentPersonQuery, Result<OutstandingIposResultDto>>
    {
        private readonly IOutstandingIPOsRawSqlRepository _outstandingIpOsRawSqlRepository;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IMeApiService _meApiService;
        private readonly IPlantProvider _plantProvider;
        private readonly ILogger<GetOutstandingIposForCurrentPersonQueryHandler> _logger;

        public GetOutstandingIposForCurrentPersonQueryHandler(
            IOutstandingIPOsRawSqlRepository outstandingIpOsRawSqlRepository,
            ICurrentUserProvider currentUserProvider,
            IMeApiService meApiService,
            IPlantProvider plantProvider,
            ILogger<GetOutstandingIposForCurrentPersonQueryHandler> logger)
        {
            _outstandingIpOsRawSqlRepository = outstandingIpOsRawSqlRepository;
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
                var plantId = _plantProvider.Plant;
                currentUserOid = _currentUserProvider.GetCurrentUserOid();
                IList<string> currentUsersFunctionalRoleCodes = default;

                var allInvitations = await _outstandingIpOsRawSqlRepository.GetOutstandingIPOsByAzureOid(plantId, currentUserOid);

                var invitationsWithFunctionalRowsExists =
                    await _outstandingIpOsRawSqlRepository.ExistsAnyOutstandingIPOsWithFunctionalRoleCodes(plantId);

                if (invitationsWithFunctionalRowsExists)
                {
                    currentUsersFunctionalRoleCodes =
                        await _meApiService.GetFunctionalRoleCodesAsync(_plantProvider.Plant);

                    if (currentUsersFunctionalRoleCodes != null && currentUsersFunctionalRoleCodes.Count > 0)
                    {
                        var invitationsByFunctionalRole = await _outstandingIpOsRawSqlRepository.GetOutstandingIPOsByFunctionalRoleCodes(plantId, currentUsersFunctionalRoleCodes);
                        allInvitations = allInvitations.Concat(invitationsByFunctionalRole).ToList();
                    }
                }

                var filteredInvitationsGrouped = allInvitations.GroupBy(i => new
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
                                AzureOid = p.AzureOid,
                                FunctionalRoleCode = p.FunctionalRoleCode,
                                Organization = p.Organization,
                                SignedBy = p.SignedBy
                            }).ToList()
                        });
                }

                return new SuccessResult<OutstandingIposResultDto>(ToOutstandingIposResultDto(filteredInvitationsList.OrderBy(i => i.Id).ToList(), currentUserOid, currentUsersFunctionalRoleCodes));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetOustandingIPOs error for user with oid {currentUserOid}");

                return new SuccessResult<OutstandingIposResultDto>(new OutstandingIposResultDto(
                        new List<OutstandingIpoDetailsDto>()));
            }
        }

        private OutstandingIposResultDto ToOutstandingIposResultDto(List<InvitationDto> currentUsersOutstandingInvitations,
            Guid currentUserOid, IList<string> currentUsersFunctionalRoleCodes)
        {
            var outstandingInvitations = new OutstandingIposResultDto(currentUsersOutstandingInvitations.Select(invitation =>
            {
                var organization = invitation.Participants.First(p =>
                    p.SignedBy == null &&
                    (p.AzureOid == currentUserOid ||
                     currentUsersFunctionalRoleCodes != null && currentUsersFunctionalRoleCodes.Contains(p.FunctionalRoleCode))).Organization;
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
