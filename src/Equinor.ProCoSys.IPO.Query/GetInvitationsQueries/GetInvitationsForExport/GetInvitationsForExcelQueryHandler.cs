using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationsQueries.GetInvitationsForExport
{
    public class GetInvitationsForExportQueryHandler : GetInvitationsQueryBase, IRequestHandler<GetInvitationsForExportQuery, Result<ExportDto>>
    {
        private readonly IReadOnlyContext _context;
        private readonly IPlantProvider _plantProvider;
        private readonly DateTime _utcNow;
        private readonly ILogger<GetInvitationsForExportQueryHandler> _logger;

        public GetInvitationsForExportQueryHandler(IReadOnlyContext context, IPlantProvider plantProvider, ILogger<GetInvitationsForExportQueryHandler> logger)
        {
            _context = context;
            _plantProvider = plantProvider;
            _utcNow = TimeService.UtcNow;
            _logger = logger;
        }

        public async Task<Result<ExportDto>> Handle(GetInvitationsForExportQuery request, CancellationToken cancellationToken)
        {
            var stopWatch = Stopwatch.StartNew();
            _logger.LogInformation("DEBUG - 90374 - GET_INVITATIONS_FOR_EXPORT START");
            var invitationsWithIncludes = await GetOrderedInvitationsWithIncludesAsync(request, cancellationToken);
            _logger.LogInformation("DEBUG - 90374 - GetOrderedInvitationsWithIncludesAsync took " + stopWatch.ElapsedMilliseconds + "ms.");

            if (!invitationsWithIncludes.Any())
            {
                return await CreateSuccessResultAsync(null, request);
            }

            stopWatch.Restart();
            var invitationsToBeExported = await CreateExportInvitationDtosAsync(invitationsWithIncludes);
            _logger.LogInformation("DEBUG - 90374 - CreateExportInvitationDtosAsync took " + stopWatch.ElapsedMilliseconds + " ms.");

            if (invitationsToBeExported.Count == 1)
            {
                await AddHistoryToSingleInvitationInList(invitationsToBeExported, cancellationToken);
            }

            return await CreateSuccessResultAsync(invitationsToBeExported, request);
        }

        private async Task AddHistoryToSingleInvitationInList(IEnumerable<ExportInvitationDto> exportInvitationDtos,
            CancellationToken cancellationToken)
        {
            var singleInvitation = exportInvitationDtos.Single();
            singleInvitation.History.AddRange(
                await GetHistoryForSingleInvitationAsync(
                    singleInvitation.Id,
                    cancellationToken));
        }

        private async Task<List<Invitation>> GetOrderedInvitationsWithIncludesAsync(GetInvitationsForExportQuery request,
            CancellationToken cancellationToken)
        {
            var stopWatch = Stopwatch.StartNew();
            var invitationForQueryDtos = CreateQueryableWithFilter(_context, request.ProjectName, request.Filter, _utcNow);
            _logger.LogInformation("DEBUG - 90374 - CreateQueryableWithFilter took " + stopWatch.ElapsedMilliseconds + " ms.");

            var orderedInvitations = await AddSorting(request.Sorting, invitationForQueryDtos).ToListAsync(cancellationToken);
            var invitationIds = orderedInvitations.Select(dto => dto.Id).ToList();

            stopWatch.Restart();
            var invitationsWithIncludes = await GetInvitationsWithIncludesAsync(_context, invitationIds, cancellationToken);
            _logger.LogInformation("DEBUG - 90374 - GetInvitationsWithIncludesAsync took " + stopWatch.ElapsedMilliseconds + " ms.");

            return orderedInvitations.Select(invitation => invitationsWithIncludes.Single(i => i.Id == invitation.Id)).ToList();
        }

        private async Task<SuccessResult<ExportDto>> CreateSuccessResultAsync(List<ExportInvitationDto> exportInvitationDtos, GetInvitationsForExportQuery request)
        {
            var filter = await CreateUsedFilterDtoAsync(request.ProjectName, request.Filter);

            _logger.LogInformation("DEBUG - 90374 - GET_INVITATIONS_FOR_EXPORT END");
            return new SuccessResult<ExportDto>(new ExportDto(exportInvitationDtos, filter));
        }

        private async Task<List<ExportHistoryDto>> GetHistoryForSingleInvitationAsync(
            int invitationId,
            CancellationToken cancellationToken)
        {
            var history = await (from h in _context.QuerySet<History>()
                    join invitation in _context.QuerySet<Invitation>() on h.ObjectGuid equals invitation.ObjectGuid
                    join createdBy in _context.QuerySet<Person>() on h.CreatedById equals createdBy.Id
                    where invitation.Id == invitationId
                    select new
                    {
                        History = h,
                        CreatedBy = createdBy
                    })
                .ToListAsync(cancellationToken);

            return history.OrderByDescending(x => x.History.CreatedAtUtc).Select(dto
                => new ExportHistoryDto(dto.History.Id, dto.History.Description, dto.History.CreatedAtUtc,
                    $"{dto.CreatedBy.FirstName} {dto.CreatedBy.LastName}")).ToList();
        }

        private static IEnumerable<ExportParticipantDto> AddParticipantsForSingleInvitation(
            Invitation invitationWithIncludes) =>
            invitationWithIncludes.Participants.Select(participant => new ExportParticipantDto(
                participant.Id,
                participant.Organization.ToString(),
                participant.Type.ToString(),
                participant.Type == IpoParticipantType.Person ? $"{participant.FirstName} {participant.LastName}" 
                    : participant.FunctionalRoleCode));

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

        private async Task<List<ExportInvitationDto>> CreateExportInvitationDtosAsync(
            IList<Invitation> invitationsWithIncludes)
        {
            var exportInvitations = new List<ExportInvitationDto>();
            foreach (var invitation in invitationsWithIncludes)
            {
                var organizer = await GetPersonNameAsync(invitation.CreatedById);
                var invitationWithIncludes = invitationsWithIncludes.Single(i => i.Id == invitation.Id);
                var participants = invitationWithIncludes.Participants.ToList();
                var exportInvitationDto = new ExportInvitationDto(
                    invitation.Id,
                    invitation.ProjectName,
                    invitation.Status,
                    invitation.Title,
                    invitation.Description,
                    invitation.Type.ToString(),
                    invitation.Location,
                    invitation.StartTimeUtc,
                    invitation.EndTimeUtc,
                    invitationWithIncludes.McPkgs.Select(mc => mc.McPkgNo),
                    invitationWithIncludes.CommPkgs.Select(c => c.CommPkgNo),
                    GetContractorRep(participants),
                    GetConstructionCompanyRep(participants),
                    invitation.CompletedAtUtc,
                    invitation.AcceptedAtUtc,
                    invitation.CreatedAtUtc,
                    organizer
                );
                exportInvitationDto.Participants.AddRange(AddParticipantsForSingleInvitation(invitation));
                exportInvitations.Add(exportInvitationDto);
            }

            return exportInvitations.ToList();
        }
    }
}
