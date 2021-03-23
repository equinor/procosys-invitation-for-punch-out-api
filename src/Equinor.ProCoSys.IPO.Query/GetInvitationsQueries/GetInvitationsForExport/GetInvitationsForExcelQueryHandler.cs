using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.Time;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationsQueries.GetInvitationsForExport
{
    public class GetInvitationsForExportQueryHandler : GetInvitationsQueryBase, IRequestHandler<GetInvitationsForExportQuery, Result<ExportDto>>
    {
        private readonly IReadOnlyContext _context;
        private readonly IPlantProvider _plantProvider;
        private readonly DateTime _utcNow;

        public GetInvitationsForExportQueryHandler(IReadOnlyContext context, IPlantProvider plantProvider)
        {
            _context = context;
            _plantProvider = plantProvider;
            _utcNow = TimeService.UtcNow;
        }

        public async Task<Result<ExportDto>> Handle(GetInvitationsForExportQuery request, CancellationToken cancellationToken)
        {
            var queryable = CreateQueryableWithFilter(_context, request.ProjectName, request.Filter, _utcNow);

            queryable = AddSorting(request.Sorting, queryable);

            var orderedDtos = queryable.ToList();

            var usedFilterDto = await CreateUsedFilterDtoAsync(request.ProjectName, request.Filter);
            if (!orderedDtos.Any())
            {
                return new SuccessResult<ExportDto>(new ExportDto(null, usedFilterDto));
            }

            var invitationIds = orderedDtos.Select(dto => dto.Id).ToList();
            var getHistoryAndParticipants = invitationIds.Count == 1;

            var invitationsWithIncludes = await (from invitation in _context.QuerySet<Invitation>()
                        .Include(i => i.Participants)
                        .Include(i => i.CommPkgs)
                        .Include(i => i.McPkgs)
                    where invitationIds.Contains(invitation.Id)
                    select invitation)
                .ToListAsync(cancellationToken);

            var exportInvitationDtos = CreateExportInvitationsDtosAsync(orderedDtos, invitationsWithIncludes).Result;

            if (getHistoryAndParticipants)
            {
                await GetHistoryForSingleInvitationAsync(
                    invitationIds.Single(),
                    exportInvitationDtos,
                    cancellationToken);

                AddParticipantsForSingleInvitation(
                    exportInvitationDtos,
                    invitationsWithIncludes);
            }

            return new SuccessResult<ExportDto>(new ExportDto(exportInvitationDtos, usedFilterDto));
        }

        private async Task GetHistoryForSingleInvitationAsync(
            int singleInvitationId,
            IList<ExportInvitationDto> exportInvitationDtos,
            CancellationToken cancellationToken)
        {
            var history = await (from h in _context.QuerySet<History>()
                    join invitation in _context.QuerySet<Invitation>() on h.ObjectGuid equals invitation.ObjectGuid
                    join createdBy in _context.QuerySet<Person>() on h.CreatedById equals createdBy.Id
                    where invitation.Id == singleInvitationId
                    select new
                    {
                        History = h,
                        CreatedBy = createdBy
                    })
                .ToListAsync(cancellationToken);

            var singleExportInvitationDto = exportInvitationDtos.Single();

            foreach (var dto in history.OrderByDescending(x => x.History.CreatedAtUtc))
            {
                singleExportInvitationDto.History.Add(new ExportHistoryDto(
                    dto.History.Id,
                    dto.History.Description,
                    dto.History.CreatedAtUtc,
                    $"{dto.CreatedBy.FirstName} {dto.CreatedBy.LastName}"));
            }
        }

        private void AddParticipantsForSingleInvitation(
            IList<ExportInvitationDto> exportInvitationDtos,
            IList<Invitation> invitationsWithIncludes)
        {
            var singleExportInvitationDto = exportInvitationDtos.Single();
            var singleInvitation = invitationsWithIncludes.Single();
            foreach (var dto in singleInvitation.Participants)
            {
                singleExportInvitationDto.Participants.Add(new ExportParticipantDto(
                    dto.Id,
                    dto.Organization.ToString(),
                    dto.Type.ToString(),
                    dto.Type == IpoParticipantType.Person ? $"{dto.FirstName} {dto.LastName}" : dto.FunctionalRoleCode));
            }
        }

        private async Task<List<Invitation>> GetInvitationsWithIncludesAsync(List<int> invitationIds,
            bool getHistoryAndParticipants, CancellationToken cancellationToken)
        {
            if (getHistoryAndParticipants)
            {
                return await (from invitation in _context.QuerySet<Invitation>()
                            .Include(t => t.Participants)
                        where invitationIds.Contains(invitation.Id)
                        select invitation)
                    .ToListAsync(cancellationToken);
            }
            return await (from invitation in _context.QuerySet<Invitation>()
                        .Include(i => i.Participants)
                        .Include(i => i.CommPkgs)
                        .Include(i => i.McPkgs)
                    where invitationIds.Contains(invitation.Id)
                    select invitation)
                .ToListAsync(cancellationToken);
        }

        private async Task<UsedFilterDto> CreateUsedFilterDtoAsync(string projectName, Filter filter)
        {
            var personInvited = await GetPersonNameAsync(filter.PersonOid);

            return new UsedFilterDto(
                _plantProvider.Plant,
                projectName,
                filter.IpoIdStartsWith,
                filter.TitleStartsWith,
                filter.McPkgNoStartsWith,
                filter.CommPkgNoStartsWith,
                filter.IpoStatuses.Select(x => x.ToString()),
                filter.PunchOutDateFromUtc,
                filter.PunchOutDateToUtc,
                filter.LastChangedAtFromUtc,
                filter.LastChangedAtToUtc,
                filter.FunctionalRoleCode,
                personInvited);
        }

        private async Task<string> GetPersonNameAsync(Guid? personOid)
        {
            if (!personOid.HasValue)
            {
                return null;
            }

            return await (from p in _context.QuerySet<Person>()
                where p.Oid == personOid.Value
                select $"{p.FirstName} {p.LastName}").SingleOrDefaultAsync();
        }

        private async Task<string> GetPersonNameAsync(int personId) =>
            await (from p in _context.QuerySet<Person>()
                where p.Id == personId
                select $"{p.FirstName} {p.LastName}").SingleAsync();

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

        private async Task<List<ExportInvitationDto>> CreateExportInvitationsDtosAsync(
            List<InvitationForQueryDto> orderedDtos,
            List<Invitation> invitationsWithIncludes)
        {
            var exportDtos = new List<ExportInvitationDto>();
            foreach (var dto in orderedDtos)
            {
                var organizer = await GetPersonNameAsync(dto.CreatedById);
                var invitationWithIncludes = invitationsWithIncludes.Single(t => t.Id == dto.Id);
                var participants = invitationWithIncludes.Participants.ToList();
                exportDtos.Add(new ExportInvitationDto(
                    dto.Id,
                    dto.ProjectName,
                    dto.Status,
                    dto.Title,
                    dto.Description,
                    dto.Type.ToString(),
                    dto.Location,
                    dto.StartTimeUtc,
                    dto.EndTimeUtc,
                    invitationWithIncludes.McPkgs.Select(mc => mc.McPkgNo).ToList(),
                    invitationWithIncludes.CommPkgs.Select(c => c.CommPkgNo).ToList(),
                    GetContractorRep(participants),
                    GetConstructionCompanyRep(participants),
                    dto.CompletedAtUtc,
                    dto.AcceptedAtUtc,
                    dto.CreatedAtUtc,
                    organizer
                ));
            }

            return exportDtos.ToList();
        }
    }
}
