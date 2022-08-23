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
        private readonly IPersonRepository _personRepository;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IPermissionCache _permissionCache;
        private readonly DateTime _utcNow;

        public GetInvitationsForExportQueryHandler(IReadOnlyContext context, IPlantProvider plantProvider, IPersonRepository personRepository, ICurrentUserProvider currentUserProvider, IPermissionCache permissionCache)
        {
            _context = context;
            _plantProvider = plantProvider;
            _personRepository = personRepository;
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

            var invitationsToBeExported = await CreateExportInvitationDtosAsync(invitationsWithIncludes);

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
            var invitationForQueryDtos = CreateQueryableWithFilter(_context, request.ProjectName, request.Filter, _utcNow, _currentUserProvider, _permissionCache, _plantProvider);

            var orderedInvitations = await AddSorting(request.Sorting, invitationForQueryDtos).ToListAsync(cancellationToken);
            var invitationIds = orderedInvitations.Select(dto => dto.Id).ToList();

            var invitationsWithIncludes = await GetInvitationsWithIncludesAsync(_context, invitationIds, cancellationToken);

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

        private async Task<IList<ExportParticipantDto>> AddParticipantsForSingleInvitationAsync(
            Invitation invitationWithIncludes)
        {
            var participants = invitationWithIncludes.Participants.Select(participant => new ExportParticipantDto(
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

            foreach (var participant in participants)
            {
                var person = participant.SignedById.HasValue
                    ? await _personRepository.GetByIdAsync(participant.SignedById.Value)
                    : null;
                participant.SignedBy = person?.UserName;
            }

            return participants;
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
                exportInvitationDto.Participants.AddRange(await AddParticipantsForSingleInvitationAsync(invitation));
                exportInvitations.Add(exportInvitationDto);
            }

            return exportInvitations.ToList();
        }
    }
}
