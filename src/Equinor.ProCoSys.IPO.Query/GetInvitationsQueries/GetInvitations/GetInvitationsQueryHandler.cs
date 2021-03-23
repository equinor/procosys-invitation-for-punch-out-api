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

        public async Task<Result<InvitationsResult>> Handle(GetInvitationsQuery request, CancellationToken token)
        {
            var queryable = CreateQueryableWithFilter(_context, request.ProjectName, request.Filter, _utcNow);

            // count before adding sorting/paging
            var maxAvailable = await queryable.CountAsync(token);

            queryable = AddSorting(request.Sorting, queryable);
            queryable = AddPaging(request.Paging, queryable);

            var orderedDtos = await queryable.ToListAsync(token);

            if (!orderedDtos.Any())
            {
                return new SuccessResult<InvitationsResult>(new InvitationsResult(maxAvailable, null));
            }

            var invitationIds = orderedDtos.Select(i => i.Id);

            var invitationWithIncludes = await (from i in _context.QuerySet<Invitation>()
                        .Include(i => i.Participants)
                        .Include(i => i.CommPkgs)
                        .Include(i => i.McPkgs)
                    where invitationIds.Contains(i.Id)
                    select i)
                .ToListAsync(token);

            var result = CreateResult(maxAvailable, orderedDtos, invitationWithIncludes);

            return new SuccessResult<InvitationsResult>(result);
        }

        private InvitationsResult CreateResult(
            int maxAvailable,
            List<InvitationForQueryDto> orderedDtos,
            List<Invitation> invitationsWithIncludes)
        {
            var invitations = orderedDtos.Select(dto =>
            {
                var invitationWithIncludes = invitationsWithIncludes.Single(t => t.Id == dto.Id);
                var participants = invitationWithIncludes.Participants.ToList();
                return new InvitationDto(dto.Id,
                    dto.ProjectName,
                    dto.Title,
                    dto.Description,
                    dto.Location,
                    dto.Type,
                    dto.Status,
                    dto.CreatedAtUtc,
                    dto.CreatedById,
                    dto.StartTimeUtc,
                    dto.EndTimeUtc,
                    dto.CompletedAtUtc,
                    dto.AcceptedAtUtc,
                    GetContractorRep(participants),
                    GetConstructionCompanyRep(participants),
                    invitationWithIncludes.McPkgs.Select(mc => mc.McPkgNo).ToList(),
                    invitationWithIncludes.CommPkgs.Select(c => c.CommPkgNo).ToList(),
                    dto.RowVersion);
            });
            var result = new InvitationsResult(maxAvailable, invitations);
            return result;
        }

        private static string NameCombiner(Participant participant)
            => $"{participant.FirstName} {participant.LastName}";

        private static string GetContractorRep(IList<Participant> participant)
        {
            var functionalRoleContractor =
                participant.SingleOrDefault(p => p.SortKey == 0 && p.Type == IpoParticipantType.FunctionalRole);
            if (functionalRoleContractor != null)
            {
                return functionalRoleContractor.FunctionalRoleCode;
            }
            return NameCombiner(participant.Single(p => p.SortKey == 0));
        }

        private static string GetConstructionCompanyRep(IList<Participant> participant)
        {
            var functionalRoleConstructionCompany =
                participant.SingleOrDefault(p => p.SortKey == 1 && p.Type == IpoParticipantType.FunctionalRole);
            if (functionalRoleConstructionCompany != null)
            {
                return functionalRoleConstructionCompany.FunctionalRoleCode;
            }
            return NameCombiner(participant.Single(p => p.SortKey == 1));
        }

        private IQueryable<InvitationForQueryDto> AddPaging(Paging paging, IQueryable<InvitationForQueryDto> enumerable)
        {
            enumerable = enumerable
                .Skip(paging.Page * paging.Size)
                .Take(paging.Size);
            return enumerable;
        }
    }
}
