using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Me;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetOutstandingIpos
{
    public class GetOutstandingIposForCurrentPersonQueryHandler : IRequestHandler<GetOutstandingIposForCurrentPersonQuery, Result<OutstandingIposResultDto>>
    {
        private readonly IReadOnlyContext _context;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IMeApiService _meApiService;
        private readonly IPlantProvider _plantProvider;

        public GetOutstandingIposForCurrentPersonQueryHandler(
            IReadOnlyContext context,
            ICurrentUserProvider currentUserProvider,
            IMeApiService meApiService,
            IPlantProvider plantProvider)
        {
            _context = context;
            _currentUserProvider = currentUserProvider;
            _meApiService = meApiService;
            _plantProvider = plantProvider;
        }

        public async Task<Result<OutstandingIposResultDto>> Handle(GetOutstandingIposForCurrentPersonQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var currentUserOid = _currentUserProvider.GetCurrentUserOid();

                //We could have filtered based on project access, however
                //the scenario that a person should lose access to a single project
                //in a plant after being added to the invitation is (according to the client)
                //so unlikely that we do not need to take it into consideration
                var nonCancelledInvitations = await (from i in _context.QuerySet<Invitation>()
                        .Include(ss => ss.Participants)
                                                     where !i.AcceptedAtUtc.HasValue
                                                     where i.Status != IpoStatus.Canceled
                                                     select i).ToListAsync(cancellationToken);

                var currentUsersOutstandingInvitations = new List<Invitation>();
                foreach (var invitation in nonCancelledInvitations)
                {
                    var currentUsersFunctionalRoleCodes =
                        await _meApiService.GetFunctionalRoleCodesAsync(_plantProvider.Plant);

                    if (UserWasInvitedAsPersonParticipant(invitation, currentUserOid))
                    {
                        currentUsersOutstandingInvitations.Add(invitation);
                    }
                    else if (UserWasInvitedAsPersonInFunctionalRole(invitation, currentUsersFunctionalRoleCodes))
                    {
                        currentUsersOutstandingInvitations.Add(invitation);
                    }
                }

                var outstandingIposResultDto = new OutstandingIposResultDto(
                    currentUsersOutstandingInvitations.Select(invitation =>
                    {
                        var organization = invitation.CompletedAtUtc.HasValue ? 
                            Organization.ConstructionCompany : Organization.Contractor;
                        return new OutstandingIpoDetailsDto
                        {
                            InvitationId = invitation.Id,
                            Description = invitation.Description,
                            Organization = organization
                        };
                    }));

                return new SuccessResult<OutstandingIposResultDto>(outstandingIposResultDto);
            }
            catch (Exception)
            {
                return new SuccessResult<OutstandingIposResultDto>(new OutstandingIposResultDto(
                        new List<OutstandingIpoDetailsDto>()));
            }
        }

        private static bool UserWasInvitedAsPersonParticipant(Invitation invitation, Guid currentUserOid)
            => invitation.Participants.Any(p => 
                p.AzureOid == currentUserOid
                && p.FunctionalRoleCode == null
                &&((!p.SignedAtUtc.HasValue 
                && p.Organization != Organization.Supplier 
                && p.Organization != Organization.External
                && p.SortKey != 1) 
                || (p.SortKey == 1 && invitation.Status == IpoStatus.Completed)));

        private static bool UserWasInvitedAsPersonInFunctionalRole(Invitation invitation, IEnumerable<string> currentUsersFunctionalRoleCodes)
        {
            var functionalRoleParticipantCodesOnInvitation = 
                invitation.Participants.Where(p => ((
                    !p.SignedAtUtc.HasValue
                    && p.Organization != Organization.Supplier
                    && p.Organization != Organization.External
                    && p.SortKey != 1) 
                    || (p.SortKey == 1 && invitation.Status == IpoStatus.Completed))
                    && p.FunctionalRoleCode != null
                    && p.Type == IpoParticipantType.FunctionalRole)
                    .Select(p => p.FunctionalRoleCode).ToList();

            return currentUsersFunctionalRoleCodes.Select(functionalRoleCode 
                => functionalRoleParticipantCodesOnInvitation.Contains(functionalRoleCode)).FirstOrDefault();
        }
    }
}
