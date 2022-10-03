using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                var outstandingIPOs = await GetOustandingIPOsNew(request, cancellationToken);
                //var outstandingIPOs = await GetOustandingIPOsOld(request, cancellationToken);
                return new SuccessResult<OutstandingIposResultDto>(outstandingIPOs);
            }
            catch (Exception)
            {
                return new SuccessResult<OutstandingIposResultDto>(new OutstandingIposResultDto(
                        new List<OutstandingIpoDetailsDto>()));
            }
        }

        public async Task<OutstandingIposResultDto> GetOustandingIPOsNew(GetOutstandingIposForCurrentPersonQuery request,
          CancellationToken cancellationToken)
        {
            var currentUserOid = _currentUserProvider.GetCurrentUserOid();

            var spElapsed = Stopwatch.StartNew();
           
            var nonCancelledInvitations =
              await (from i in _context.QuerySet<Invitation>()
                     join p in _context.QuerySet<Participant>() on i.Id equals EF.Property<int>(p, "InvitationId")
                     where i.Status != IpoStatus.Canceled
                     select new
                     {
                         Id = i.Id,
                         Description = i.Description,
                         Status = i.Status,

                         ParticipantId = p.Id,
                         AzureOid = p.AzureOid,
                         FunctionalRoleCode = p.FunctionalRoleCode,
                         Organization = p.Organization,
                         SignedAtUtc = p.SignedAtUtc,
                         SortKey = p.SortKey,
                         Type = p.Type,
                         SignedBy = p.SignedBy

                     })
                     .ToListAsync(cancellationToken);
           

            var elapsedProjection = spElapsed.ElapsedMilliseconds;
            spElapsed.Restart();

            var nonCancelledInvitationsGrouped = new List<InvitationDto>();

            foreach (var iGroup in nonCancelledInvitations
                .GroupBy(i => new { i.Id, i.Description, i.Status }))
            {
                var invitation = iGroup.Key;

                nonCancelledInvitationsGrouped.Add(
                    new InvitationDto()
                    {

                        Id = iGroup.Key.Id,
                        Description = iGroup.Key.Description,
                        Status = iGroup.Key.Status,
                        Participants = iGroup.Select(p => new InvitationParticipantDto()
                        {
                            ParticipantId = p.ParticipantId,
                            AzureOid = p.AzureOid,
                            FunctionalRoleCode = p.FunctionalRoleCode,
                            Organization = p.Organization,
                            SignedAtUtc = p.SignedAtUtc,
                            SignedBy = p.SignedBy,
                            SortKey = p.SortKey,
                            Type = p.Type
                        }).ToList(),

                    }
                    );
            }


           

            var currentUsersOutstandingInvitations = new List<InvitationDto>();

            var listHasFunctionalRoles =
                nonCancelledInvitations.Any(i => i.FunctionalRoleCode != null);

            IList<string> currentUsersFunctionalRoleCodes = null;

            if (listHasFunctionalRoles)
            {
                currentUsersFunctionalRoleCodes = await _meApiService.GetFunctionalRoleCodesAsync(_plantProvider.Plant);
            }

            foreach (var invitation in nonCancelledInvitationsGrouped)
            {

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

            var elapsedinMemWork = spElapsed.ElapsedMilliseconds;

            return outstandingIposResultDto;
        }

        private static bool UserWasInvitedAsPersonParticipant(InvitationDto invitation, Guid currentUserOid)
         => invitation.Participants.Any(p =>
                p.AzureOid == currentUserOid
                && p.FunctionalRoleCode == null
                && ((!p.SignedAtUtc.HasValue
                && p.Organization != Organization.Supplier
                && p.Organization != Organization.External
                && p.SortKey != 1)
                || (p.SortKey == 1 && invitation.Status == IpoStatus.Completed)));

        private static bool UserWasInvitedAsPersonInFunctionalRole(InvitationDto invitation, IEnumerable<string> currentUsersFunctionalRoleCodes)
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

        public async Task<OutstandingIposResultDto> GetOustandingIPOsOld(GetOutstandingIposForCurrentPersonQuery request,
            CancellationToken cancellationToken)
        {

            var currentUserOid = _currentUserProvider.GetCurrentUserOid();

            //We could have filtered based on project access, however
            //the scenario that a person should lose access to a single project
            //in a plant after being added to the invitation is (according to the client)
            //so unlikely that we do not need to take it into consideration
            var nonCancelledInvitations = await (from i in _context.QuerySet<Invitation>()
                    .Include(ss => ss.Participants)
                                                 where i.Status != IpoStatus.Canceled
                                                 select i).ToListAsync(cancellationToken);

            var currentUsersOutstandingInvitations = new List<Invitation>();

            var listHasFunctionalRoles =
                nonCancelledInvitations.Any(i => i.Participants.Any(p => p.FunctionalRoleCode != null));

            IList<string> currentUsersFunctionalRoleCodes = new List<string>();
            if (listHasFunctionalRoles)
            {
                currentUsersFunctionalRoleCodes = await _meApiService.GetFunctionalRoleCodesAsync(_plantProvider.Plant);
            }

            foreach (var invitation in nonCancelledInvitations)
            {

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

            return outstandingIposResultDto;

        }

        private static bool UserWasInvitedAsPersonParticipant(Invitation invitation, Guid currentUserOid)
            => invitation.Participants.Any(p =>
                p.AzureOid == currentUserOid
                && p.FunctionalRoleCode == null
                && ((!p.SignedAtUtc.HasValue
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

    public class InvitationDto
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public IpoStatus Status { get; set; }

        public List<InvitationParticipantDto> Participants { get; set; }

    }

    public class InvitationParticipantDto
    {
        public int ParticipantId { get; set; }

        public Guid? AzureOid { get; set; }

        public string FunctionalRoleCode { get; set; }

        public DateTime? SignedAtUtc { get; set; }

        public Organization Organization { get; set; }

        public int SortKey { get; set; }

        public IpoParticipantType Type { get; set; }

        public int? SignedBy { get; set; }

    }
}
