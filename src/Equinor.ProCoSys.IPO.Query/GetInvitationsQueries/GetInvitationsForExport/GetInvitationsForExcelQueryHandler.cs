using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.Time;
using Equinor.ProCoSys.IPO.Query.GetInvitations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationsQueries.GetInvitationsForExport
{
    public class GetInvitationsForExportQueryHandler : GetInvitationsQueryBase, IRequestHandler<GetInvitationsForExportQuery, Result<ExportDto>>
    {
        private readonly string _noDataFoundMarker = "null";
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

            queryable = AddSorting(request.Sorting, queryable.AsEnumerable()).AsQueryable();

            var orderedDtos = await queryable.ToListAsync(cancellationToken);

            var usedFilterDto = await CreateUsedFilterDtoAsync(request.ProjectName, request.Filter);
            if (!orderedDtos.Any())
            {
                return new SuccessResult<ExportDto>(new ExportDto(null, usedFilterDto));
            }

            var tagsIds = orderedDtos.Select(dto => dto.Id).ToList();

            var invitationsWithIncludes = await GetInvitationsWithIncludesAsync(tagsIds, cancellationToken);
            

            var exportTagDtos = CreateExportInvitationsDtos(
                orderedDtos,
                invitationsWithIncludes);

       
            return new SuccessResult<ExportDto>(new ExportDto(exportTagDtos, usedFilterDto));
        }

        private async Task<List<Invitation>> GetInvitationsWithIncludesAsync(List<int> invitationIds, CancellationToken cancellationToken)
        {
            // get tags again, including Requirements, Actions and Attachments. See comment in CreateQueryableWithFilter regarding Include and EF
            var invitationsWithIncludes = await (from tag in _context.QuerySet<Invitation>()
                        .Include(t => t.Attachments)
                    where invitationIds.Contains(tag.Id)
                    select tag)
                .ToListAsync(cancellationToken);

            return invitationsWithIncludes;
        }

        private async Task<UsedFilterDto> CreateUsedFilterDtoAsync(string projectName, Filter filter)
        {
            var projectDescription = await GetProjectDescriptionAsync(projectName);
            var requirementTypeTitles = await GetRequirementTypeTitlesAsync(filter.RequirementTypeIds);
            var responsibleCodes = await GetResponsibleCodesAsync(filter.ResponsibleIds);
            var modeTitles = await GetModeTitlesAsync(filter.ModeIds);
            var journeyTitles = await GetJourneyTitlesAsync(filter.JourneyIds);

            return new UsedFilterDto(
                filter.ActionStatus.GetDisplayValue(),
                filter.AreaCodes,
                filter.CallOffStartsWith,
                filter.CommPkgNoStartsWith,
                filter.DisciplineCodes,
                filter.DueFilters.Select(v => v.GetDisplayValue()), 
                journeyTitles,
                filter.McPkgNoStartsWith,
                modeTitles,
                filter.PreservationStatus.GetDisplayValue(),
                projectDescription,
                _plantProvider.Plant,
                projectName,
                filter.PurchaseOrderNoStartsWith,
                requirementTypeTitles,
                responsibleCodes,
                filter.StorageAreaStartsWith,
                filter.TagFunctionCodes,
                filter.TagNoStartsWith,
                filter.VoidedFilter.GetDisplayValue());
        }

        private async Task<string> GetProjectDescriptionAsync(string projectName) 
            => await (from p in _context.QuerySet<Project>()
                where p.Name == projectName
                select p.Description).SingleOrDefaultAsync();

        private async Task<List<string>> GetJourneyTitlesAsync(IList<int> journeyIds)
        {
            if (!journeyIds.Any())
            {
                return new List<string>();
            }

            return await (from j in _context.QuerySet<Journey>()
                where journeyIds.Contains(j.Id)
                select j.Title).ToListAsync();
        }

        private async Task<List<string>> GetModeTitlesAsync(IList<int> modeIds)
        {
            if (!modeIds.Any())
            {
                return new List<string>();
            }

            return await (from m in _context.QuerySet<Mode>()
                where modeIds.Contains(m.Id)
                select m.Title).ToListAsync();
        }




        private IList<ExportInvitationDto> CreateExportInvitationsDtos(
            List<InvitationForQueryDto> orderedDtos,
            List<Invitation> invitationsWithIncludes)
        {
            var invitations = orderedDtos.Select(dto =>
            {
                var tagWithIncludes = invitationsWithIncludes.Single(t => t.Id == dto.Id);
                var orderedRequirements = tagWithIncludes.OrderedRequirements().ToList();
                var requirementTitles = orderedRequirements
                    .Select(r => reqDefs.Single(rd => rd.Id == r.RequirementDefinitionId).Title)
                    .ToList();

                int? nextDueWeeks = null;
                var nextDueAsYearAndWeek = string.Empty;

                var firstUpcomingRequirement = orderedRequirements.FirstOrDefault();
                if (firstUpcomingRequirement != null)
                {
                    nextDueWeeks = firstUpcomingRequirement.GetNextDueInWeeks();
                    nextDueAsYearAndWeek = firstUpcomingRequirement.NextDueTimeUtc?.FormatAsYearAndWeekString();
                }

                var journeyWithSteps = journeysWithSteps.Single(j => j.Id == dto.JourneyId);
                var step = journeyWithSteps.Steps.Single(s => s.Id == dto.StepId);

                var openActionsCount = tagWithIncludes.Actions.Count(a => !a.IsClosed);
                var overdueActionsCount = tagWithIncludes.Actions.Count(a => a.IsOverDue());

                var orderedActions = tagWithIncludes
                    .Actions
                    .OrderBy(t => t.IsClosed.Equals(true))
                    .ThenByDescending(t => t.DueTimeUtc.HasValue)
                    .ThenBy(t => t.DueTimeUtc)
                    .ThenBy(t => t.ModifiedAtUtc)
                    .ThenBy(t => t.CreatedAtUtc);

                return new ExportInvitationDto(
                    );
            });

            return invitations.ToList();
        }
    }
}
