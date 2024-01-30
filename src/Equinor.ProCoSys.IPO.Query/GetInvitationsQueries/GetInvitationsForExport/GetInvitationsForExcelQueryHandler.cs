using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Common.Time;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceResult;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Common;
using Microsoft.Extensions.Logging;
using Equinor.ProCoSys.IPO.Infrastructure.Repositories.ExportIPOs;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationsQueries.GetInvitationsForExport
{
    public class GetInvitationsForExportQueryHandler : GetInvitationsQueryBase, IRequestHandler<GetInvitationsForExportQuery, Result<ExportDto>>
    {
        private readonly IExportIpoRepository _exportIpoSqlRepository;
        private readonly ILogger<GetInvitationsForExportQuery> _logger;
        private readonly IReadOnlyContext _context;
        private readonly IPlantProvider _plantProvider;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IPermissionCache _permissionCache;
        private readonly DateTime _utcNow;

        public GetInvitationsForExportQueryHandler(IReadOnlyContext context, IPlantProvider plantProvider, ICurrentUserProvider currentUserProvider, IPermissionCache permissionCache, IExportIpoRepository exportIpoSqlRepository, ILogger<GetInvitationsForExportQuery> logger)
        {
            _exportIpoSqlRepository = exportIpoSqlRepository;
            _logger = logger;
            _context = context;
            _plantProvider = plantProvider;
            _utcNow = TimeService.UtcNow;
            _currentUserProvider = currentUserProvider;
            _permissionCache = permissionCache;
        }

        public async Task<Result<ExportDto>> Handle(GetInvitationsForExportQuery request, CancellationToken cancellationToken)
        {
            var invitationsWithIncludes = await GetOrderedInvitationsWithIncludesAsync(request, cancellationToken);

            if (!invitationsWithIncludes.Any())
            {
                return await CreateSuccessResultAsync(null, request);
            }

            _logger.LogInformation("Export to Excel. Format retrieved result.");
            var invitationsToBeExported = await CreateExportInvitationDtosAsync(invitationsWithIncludes);

            if (invitationsToBeExported.Count == 1)
            {
                _logger.LogInformation("Export to Excel. Add history to single invitation.");
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
            _logger.LogInformation("Export to excel. Creating queryable with filter...");

            var invitationForQueryDtos = CreateQueryableWithFilter(_context, request.ProjectName, request.Filter, _utcNow, _currentUserProvider, _permissionCache, _plantProvider);

            _logger.LogInformation("Export to excel. Add sorting to queryable...");
            var orderedInvitations = await AddSorting(request.Sorting, invitationForQueryDtos).ToListAsync(cancellationToken);

            _logger.LogInformation("Export to excel. Retrieving invitation ids from ordered invitations...");
            var invitationIds = orderedInvitations.Select(dto => dto.Id).ToList();

            _logger.LogInformation("Export to excel. Get invitations with includes...");

            var invitationsWithIncludes = await _exportIpoSqlRepository.GetInvitationsWithIncludesAsync(invitationIds, _plantProvider, cancellationToken);

            _logger.LogInformation("Export to excel. Extract invitations with includes.");
            return orderedInvitations.Select(invitation => invitationsWithIncludes.Single(i => i.Id == invitation.Id)).ToList();
        }

        private async Task<SuccessResult<ExportDto>> CreateSuccessResultAsync(List<ExportInvitationDto> exportInvitationDtos, GetInvitationsForExportQuery request)
        {
            var filter = await CreateUsedFilterDtoAsync(request.ProjectName, request.Filter);
            return new SuccessResult<ExportDto>(new ExportDto(exportInvitationDtos, filter));
        }

        private async Task<List<ExportHistoryDto>> GetHistoryForSingleInvitationAsync(
            int invitationId,
            CancellationToken cancellationToken)
        {
            var history = await (from h in _context.QuerySet<History>()
                                 join invitation in _context.QuerySet<Invitation>() on h.SourceGuid equals invitation.Guid
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

        private IList<ExportParticipantDto> CreateParticipantDtosForInvitation(Invitation invitationWithIncludes)
            => invitationWithIncludes.Participants.Select(participant => new ExportParticipantDto(
                participant.Id,
                participant.Organization.ToString(),
                participant.Type.ToString(),
                participant.Type == IpoParticipantType.Person
                    ? $"{participant.FirstName} {participant.LastName}"
                    : participant.FunctionalRoleCode,
                participant.Attended,
                participant.Note,
                participant.SignedAtUtc,
                null,
                participant.SignedBy)).ToList();


        private async Task FillSignedByAsync(List<ExportInvitationDto> exportInvitations)
        {
            var allParticipantDtos = exportInvitations.SelectMany(p => p.Participants).ToList();

            var participantIds = allParticipantDtos
                .Where(p => p.SignedById.HasValue)
                .Select(p => p.SignedById.Value)
                .Distinct().ToList();

            if (participantIds.Any())
            {
                var participantPersons = await GetPersonsByIdsAsync(participantIds);
                allParticipantDtos.ForEach(p =>
                {
                    if (p.SignedById.HasValue)
                    {
                        p.SignedBy = participantPersons.Single(x => x.Id == p.SignedById.Value).UserName;
                    }
                });
            }
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
                          where p.Guid == personOid.Value
                          select $"{p.FirstName} {p.LastName}").SingleOrDefaultAsync();
        }

        private async Task<List<Person>> GetPersonsByIdsAsync(List<int> personIds) =>
            await (from p in _context.QuerySet<Person>()
                   where personIds.Contains(p.Id)
                   select p).ToListAsync();

        private async Task<List<ExportInvitationDto>> CreateExportInvitationDtosAsync(
            IList<Invitation> invitationsWithIncludes)
        {
            var organizerIds = invitationsWithIncludes.Select(i => i.CreatedById).Distinct().ToList();
            var organizers = await GetPersonsByIdsAsync(organizerIds);
            var organizersDict = organizers.ToDictionary(o => o.Id);

            var invitationProjectIds = invitationsWithIncludes.Select(i => i.ProjectId).ToList();

            var projects = await _context.QuerySet<Project>()
                .Where(p => invitationProjectIds.Contains(p.Id))
                .ToListAsync();

            var exportInvitations = new List<ExportInvitationDto>();
            foreach (var invitation in invitationsWithIncludes)
            {
                var project = projects.Single(x => x.Id == invitation.ProjectId);

                if (project is null)
                {
                    throw new ArgumentNullException(nameof(project));
                }

                var organizer = organizersDict[invitation.CreatedById];
                var invitationWithIncludes = invitationsWithIncludes.Single(i => i.Id == invitation.Id);
                var participants = invitationWithIncludes.Participants.ToList();
                var exportInvitationDto = new ExportInvitationDto(
                    invitation.Id,
                    project.Name,
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
                    organizer.GetFullName()
                );
                exportInvitationDto.Participants.AddRange(CreateParticipantDtosForInvitation(invitation));
                exportInvitations.Add(exportInvitationDto);
            }
            await FillSignedByAsync(exportInvitations);

            return exportInvitations.ToList();
        }
    }
}
