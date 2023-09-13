using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Me;
using Equinor.ProCoSys.IPO.Infrastructure.Repositories.OutstandingIPOs;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetOutstandingIpos
{
    public class GetOutstandingIposForCurrentPersonQueryHandler : IRequestHandler<GetOutstandingIposForCurrentPersonQuery, Result<OutstandingIposResultDto>>
    {
        private readonly IOutstandingIpoRepository _outstandingIpOsRawSqlRepository;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IMeApiService _meApiService;
        private readonly IPlantProvider _plantProvider;
        private readonly ILogger<GetOutstandingIposForCurrentPersonQueryHandler> _logger;

        public GetOutstandingIposForCurrentPersonQueryHandler(
            IOutstandingIpoRepository outstandingIpOsRawSqlRepository,
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

                var invitationsByAzureOid = await _outstandingIpOsRawSqlRepository.GetOutstandingIposByAzureOid(plantId, currentUserOid);

                var currentUsersFunctionalRoleCodes =
                        await _meApiService.GetFunctionalRoleCodesAsync(_plantProvider.Plant);

                var invitationsByFunctionalRole = await _outstandingIpOsRawSqlRepository.GetOutstandingIposByFunctionalRoleCodes(plantId, currentUsersFunctionalRoleCodes);
                var allInvitations = invitationsByAzureOid.Concat(invitationsByFunctionalRole).ToList();

                var invitationsGrouped = allInvitations.GroupBy(i => new
                {
                    i.Id,
                    i.Description
                }).ToList();

                var ipoDetails = invitationsGrouped.Select(group =>
                    {
                        var participants = group.Select(p => new ParticipantDto()
                        {
                            AzureOid = p.AzureOid,
                            FunctionalRoleCode = p.FunctionalRoleCode,
                            Organization = p.Organization,
                            SignedBy = p.SignedBy
                        });

                        return new OutstandingIpoDetailsDto()
                        {
                            InvitationId = group.Key.Id,
                            Description = group.Key.Description,
                            Organization = participants.First(p =>
                                p.SignedBy == null
                             ).Organization
                        };
                    }
                ).OrderBy(i => i.InvitationId);

                return new SuccessResult<OutstandingIposResultDto>(new OutstandingIposResultDto(ipoDetails));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetOustandingIPOs error for user with oid {currentUserOid}");

                return new SuccessResult<OutstandingIposResultDto>(new OutstandingIposResultDto(
                        new List<OutstandingIpoDetailsDto>()));
            }
        }
    }
}
