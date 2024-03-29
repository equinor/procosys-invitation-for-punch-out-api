﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Microsoft.EntityFrameworkCore;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationsQueries
{
    public abstract class GetInvitationsQueryBase
    {
        protected IQueryable<InvitationForQueryDto> CreateQueryableWithFilter(
            IReadOnlyContext context, 
            string projectName, 
            Filter filter, 
            DateTime utcNow, 
            ICurrentUserProvider currentUserProvider, 
            IPermissionCache permissionCache, 
            IPlantProvider plantProvider)
        {

            var projectNames = new List<string>();

            if (projectName == null)
            {
                var currentUserOid = currentUserProvider.GetCurrentUserOid();
                var accessableProjects = permissionCache.GetProjectsForUserAsync(plantProvider.Plant, currentUserOid).GetAwaiter().GetResult();
                var accessableProjectNames = accessableProjects.Select(p => p.Name);
                projectNames.AddRange(accessableProjectNames);
            }
            else
            {
                projectNames.Add(projectName);
            }

            var startOfThisWeekUtc = DateTime.MinValue;
            var startOfNextWeekUtc = DateTime.MinValue;
            var startOfTwoWeeksUtc = DateTime.MinValue;
            if (filter.PunchOutDates.Any())
            {
                startOfThisWeekUtc = utcNow.StartOfWeek();
                startOfNextWeekUtc = startOfThisWeekUtc.AddWeeks(1);
                startOfTwoWeeksUtc = startOfThisWeekUtc.AddWeeks(2);
            }

            var ipoIdStartWith = GetIpoIdStartWith(filter.IpoIdStartsWith);
            var filterPunchOutDates = new List<int>();
            if (filter.PunchOutDates.Any())
            {
                filterPunchOutDates = filter.PunchOutDates.Select(x => (int)x).ToList();
            }

            var filterIpoStatuses = new List<int>();
            if (filter.IpoStatuses.Any())
            {
                filterIpoStatuses = filter.IpoStatuses.Select(x => (int)x).ToList();
            }
            // No need to specify that case should be ignored here, e.g. by using slow performing ToUpper(), since related database is created with a case insensitive collation.
            // EF Core migrations uses the default collation SQL_Latin1_General_CP1_CI_AS which is a case-insensitive collation.

            var queryable = from invitation in context.QuerySet<Invitation>()
                            join project in context.QuerySet<Project>() on invitation.ProjectId equals project.Id
                            where projectNames.Contains(project.Name) && invitation.ProjectId == project.Id &&
                     (!filter.PunchOutDates.Any() ||
                         (filterPunchOutDates.Any(pd => pd == (int)PunchOutDateFilterType.Overdue) && invitation.StartTimeUtc < utcNow) ||
                         (filterPunchOutDates.Any(pd => pd == (int)PunchOutDateFilterType.ThisWeek) && invitation.StartTimeUtc >= startOfThisWeekUtc && invitation.StartTimeUtc < startOfNextWeekUtc) ||
                         (filterPunchOutDates.Any(pd => pd == (int)PunchOutDateFilterType.NextWeek) && invitation.StartTimeUtc >= startOfNextWeekUtc && invitation.StartTimeUtc < startOfTwoWeeksUtc)) &&
                     (!filter.IpoStatuses.Any() ||
                      (filterIpoStatuses.Any(s => s == (int)IpoStatus.Planned) && (int)invitation.Status == (int)IpoStatus.Planned) ||
                      (filterIpoStatuses.Any(s => s == (int)IpoStatus.ScopeHandedOver) && (int)invitation.Status == (int)IpoStatus.ScopeHandedOver) ||
                      (filterIpoStatuses.Any(s => s == (int)IpoStatus.Completed) && (int)invitation.Status == (int)IpoStatus.Completed) ||
                      (filterIpoStatuses.Any(s => s == (int)IpoStatus.Accepted) && (int)invitation.Status == (int)IpoStatus.Accepted) ||
                      (filterIpoStatuses.Any(s => s == (int)IpoStatus.Canceled) && (int)invitation.Status == (int)IpoStatus.Canceled)) &&
                         (string.IsNullOrEmpty(filter.IpoIdStartsWith) ||
                      invitation.Id.ToString().StartsWith(ipoIdStartWith)) &&
                         (string.IsNullOrEmpty(filter.CommPkgNoStartsWith) ||
                          invitation.CommPkgs.Any(c => c.CommPkgNo.StartsWith(filter.CommPkgNoStartsWith)) ||
                          invitation.McPkgs.Any(mc => mc.CommPkgNo.StartsWith(filter.CommPkgNoStartsWith))) &&
                         (string.IsNullOrEmpty(filter.McPkgNoStartsWith) ||
                          invitation.McPkgs.Any(mc => mc.McPkgNo.StartsWith(filter.McPkgNoStartsWith))) &&
                         (string.IsNullOrEmpty(filter.TitleStartsWith) ||
                         invitation.Title.StartsWith(filter.TitleStartsWith)) &&
                         (filter.PersonOid == null ||
                             invitation.Participants.Any(p => p.AzureOid == filter.PersonOid)) &&
                         (filter.FunctionalRoleCode == null ||
                             invitation.Participants.Any(p => p.FunctionalRoleCode.Equals(filter.FunctionalRoleCode))) &&
                                  (filter.PunchOutDateFromUtc == null ||
                                   invitation.StartTimeUtc >= filter.PunchOutDateFromUtc) &&
                                  (filter.PunchOutDateToUtc == null ||
                                   invitation.EndTimeUtc <= filter.PunchOutDateToUtc) &&
                                  (filter.LastChangedAtFromUtc == null ||
                                   (invitation.ModifiedAtUtc ?? invitation.CreatedAtUtc) >= filter.LastChangedAtFromUtc) &&
                                  (filter.LastChangedAtToUtc == null ||
                                   (invitation.ModifiedAtUtc ?? invitation.CreatedAtUtc) <= filter.LastChangedAtToUtc)
                            select new InvitationForQueryDto
                            {
                                Id = invitation.Id,
                                ProjectName = project.Name,
                                Title = invitation.Title,
                                Description = invitation.Description,
                                Status = invitation.Status,
                                Type = invitation.Type,
                                CreatedAtUtc = invitation.CreatedAtUtc,
                                CreatedById = invitation.CreatedById,
                                StartTimeUtc = invitation.StartTimeUtc,
                                EndTimeUtc = invitation.EndTimeUtc,
                                CompletedAtUtc = invitation.CompletedAtUtc,
                                AcceptedAtUtc = invitation.AcceptedAtUtc,
                                RowVersion = invitation.RowVersion.ConvertToString()
                            };
            return queryable;
        }

        protected static IQueryable<InvitationForQueryDto> AddSorting(Sorting sorting, IQueryable<InvitationForQueryDto> invitationForQueryDtos)
        {
            switch (sorting.Direction)
            {
                default:
                    switch (sorting.Property)
                    {
                        case SortingProperty.Type:
                            invitationForQueryDtos = invitationForQueryDtos.OrderBy(dto => dto.Type);
                            break;
                        case SortingProperty.Status:
                            invitationForQueryDtos = invitationForQueryDtos.OrderBy(dto => dto.Status);
                            break;
                        case SortingProperty.IpoNo:
                            invitationForQueryDtos = invitationForQueryDtos.OrderBy(dto => dto.Id);
                            break;
                        case SortingProperty.Title:
                            invitationForQueryDtos = invitationForQueryDtos.OrderBy(dto => dto.Title);
                            break;
                        case SortingProperty.PunchOutDateUtc:
                            invitationForQueryDtos = invitationForQueryDtos.OrderBy(dto => dto.StartTimeUtc);
                            break;
                        case SortingProperty.CompletedAtUtc:
                            invitationForQueryDtos = invitationForQueryDtos.OrderBy(dto => dto.CompletedAtUtc);
                            break;
                        case SortingProperty.AcceptedAtUtc:
                            invitationForQueryDtos = invitationForQueryDtos.OrderBy(dto => dto.AcceptedAtUtc);
                            break;
                        case SortingProperty.ProjectName:
                            invitationForQueryDtos = invitationForQueryDtos.OrderBy( dto => dto.ProjectName);
                            break;
                        default:
                            invitationForQueryDtos = invitationForQueryDtos.OrderBy(dto => dto.Id);
                            break;
                    }

                    break;
                case SortingDirection.Desc:
                    switch (sorting.Property)
                    {
                        case SortingProperty.Type:
                            invitationForQueryDtos = invitationForQueryDtos.OrderByDescending(dto => dto.Type);
                            break;
                        case SortingProperty.Status:
                            invitationForQueryDtos = invitationForQueryDtos.OrderByDescending(dto => dto.Status);
                            break;
                        case SortingProperty.IpoNo:
                            invitationForQueryDtos = invitationForQueryDtos.OrderByDescending(dto => dto.Id);
                            break;
                        case SortingProperty.Title:
                            invitationForQueryDtos = invitationForQueryDtos.OrderByDescending(dto => dto.Title);
                            break;
                        case SortingProperty.PunchOutDateUtc:
                            invitationForQueryDtos = invitationForQueryDtos.OrderByDescending(dto => dto.StartTimeUtc);
                            break;
                        case SortingProperty.CompletedAtUtc:
                            invitationForQueryDtos = invitationForQueryDtos.OrderByDescending(dto => dto.CompletedAtUtc);
                            break;
                        case SortingProperty.AcceptedAtUtc:
                            invitationForQueryDtos = invitationForQueryDtos.OrderByDescending(dto => dto.AcceptedAtUtc);
                            break;
                        case SortingProperty.ProjectName:
                            invitationForQueryDtos = invitationForQueryDtos.OrderByDescending(dto => dto.ProjectName);
                            break;
                        default:
                            invitationForQueryDtos = invitationForQueryDtos.OrderByDescending(dto => dto.Id);
                            break;
                    }
                    break;
            }

            return invitationForQueryDtos;
        }

        private static string GetIpoIdStartWith(string filterString)
        {
            if (filterString == null)
            {
                return null;
            }
            try
            {
                return int.Parse(filterString).ToString();
            }
            catch
            {
                if (filterString.Length > 3 && filterString.Substring(0, 4).ToUpper() == "IPO-")
                {
                    return filterString.Substring(4);
                }

                return null;
            }
        }

        private static string NameCombiner(Participant participant)
            => $"{participant.FirstName} {participant.LastName}";

        protected static string GetContractorRep(IList<Participant> participant)
        {
            var functionalRoleContractor =
                participant.SingleOrDefault(p => p.SortKey == 0 && p.Type == IpoParticipantType.FunctionalRole);
            if (functionalRoleContractor != null)
            {
                return functionalRoleContractor.FunctionalRoleCode;
            }
            return NameCombiner(participant.Single(p => p.SortKey == 0));
        }

        protected static string GetConstructionCompanyRep(IList<Participant> participant)
        {
            var functionalRoleConstructionCompany =
                participant.SingleOrDefault(p => p.SortKey == 1 && p.Type == IpoParticipantType.FunctionalRole);
            if (functionalRoleConstructionCompany != null)
            {
                return functionalRoleConstructionCompany.FunctionalRoleCode;
            }
            return NameCombiner(participant.Single(p => p.SortKey == 1));
        }

        protected static IEnumerable<string> GetCommissioningReps(IList<Participant> participants)
        {
            var allCommissioningParticipants = new List<string>();
            var functionalRoleParticipants = participants.Where(p =>
                p.Organization == Organization.Commissioning && p.Type == IpoParticipantType.FunctionalRole).ToList();

            AddFunctionalRoleParticipants(functionalRoleParticipants, allCommissioningParticipants);

            var personParticipants = participants.Where(p => 
                    p.Organization == Organization.Commissioning
                    && p.Type == IpoParticipantType.Person)
                .ToList();

            AddPersonParticipants(personParticipants, allCommissioningParticipants);
            return allCommissioningParticipants;
        }

        protected static IEnumerable<string> GetOperationReps(IList<Participant> participants)
        {
            var allOperationParticipants = new List<string>();
            var functionalRoleParticipants =
                participants.Where(p => p.Organization == Organization.Operation 
                                        && p.Type == IpoParticipantType.FunctionalRole).ToList();

            AddFunctionalRoleParticipants(functionalRoleParticipants, allOperationParticipants);

            var personParticipants = participants.Where(
                p => p.Organization == Organization.Operation
                && p.Type == IpoParticipantType.Person)
                .ToList();

            AddPersonParticipants(personParticipants, allOperationParticipants);
            return allOperationParticipants;
        }

        protected static IEnumerable<string> GetTechnicalIntegrityReps(IList<Participant> participants)
        {
            var allTechnicalIntegrityParticipants = new List<string>();
            var functionalRoleParticipants = participants.Where(p =>
                    p.Organization == Organization.TechnicalIntegrity
                    && p.Type == IpoParticipantType.FunctionalRole)
                .ToList();

            AddFunctionalRoleParticipants(functionalRoleParticipants, allTechnicalIntegrityParticipants);

            var personParticipants = participants.Where(
                    p => p.Organization == Organization.TechnicalIntegrity 
                    && p.Type == IpoParticipantType.Person)
                .ToList();

            AddPersonParticipants(personParticipants, allTechnicalIntegrityParticipants);
            return allTechnicalIntegrityParticipants;
        }

        protected static IEnumerable<string> GetSupplierReps(IList<Participant> participants)
        {
            var allSupplierParticipants = new List<string>();
            var functionalRoleParticipants =
                participants.Where(p => p.Organization == Organization.Supplier 
                                        && p.Type == IpoParticipantType.FunctionalRole).ToList();

            AddFunctionalRoleParticipants(functionalRoleParticipants, allSupplierParticipants);

            var personParticipants = participants.Where(p => 
                p.Organization == Organization.Supplier 
                && p.Type == IpoParticipantType.Person)
                .ToList();

            AddPersonParticipants(personParticipants, allSupplierParticipants);
            return allSupplierParticipants;
        }

        protected static IEnumerable<string> GetExternalGuests(IEnumerable<Participant> participants)
        {
            var externalGuestParticipants = participants.Where(p => p.Organization == Organization.External).ToList();

            var externalGuestParticipantEmails = new List<string>();
            foreach (var participant in externalGuestParticipants)
            {
                externalGuestParticipantEmails.Add(participant.Email);
            }

            return externalGuestParticipantEmails;
        }

        protected static IEnumerable<string> GetAdditionalContractorReps(IList<Participant> participants)
        {
            var allAdditionalContractorParticipants = new List<string>();

            var functionalRoleParticipants = participants
                .Where(p => p.Organization == Organization.Contractor
                            && p.SortKey != 0
                            && p.Type == IpoParticipantType.FunctionalRole).ToList();

            AddFunctionalRoleParticipants(functionalRoleParticipants, allAdditionalContractorParticipants);

            var personParticipants = participants
                .Where(p => p.Organization == Organization.Contractor 
                            && p.SortKey != 0
                            && p.Type == IpoParticipantType.Person).ToList();

            AddPersonParticipants(personParticipants, allAdditionalContractorParticipants);
            return allAdditionalContractorParticipants;
        }

        protected static IEnumerable<string> GetAdditionalConstructionCompanyReps(IList<Participant> participants)
        {
            var allAdditionalConstructionCompanyParticipants = new List<string>();

            var functionalRoleParticipants = participants
                .Where(p => p.Organization == Organization.ConstructionCompany
                            && p.SortKey != 1
                            && p.Type == IpoParticipantType.FunctionalRole).ToList();

            AddFunctionalRoleParticipants(functionalRoleParticipants, allAdditionalConstructionCompanyParticipants);

            var personParticipants = participants
                .Where(p => p.Organization == Organization.ConstructionCompany 
                            && p.SortKey != 1
                            && p.Type == IpoParticipantType.Person).ToList();

            AddPersonParticipants(personParticipants, allAdditionalConstructionCompanyParticipants);
            return allAdditionalConstructionCompanyParticipants;
        }

        private static void AddPersonParticipants(IList<Participant> personParticipants, List<string> totalListOfParticipants)
        {
            if (personParticipants.Count > 0)
            {
                foreach (var participantName in personParticipants.Select(NameCombiner))
                {
                    totalListOfParticipants.Add(participantName);
                }
            }
        }

        private static void AddFunctionalRoleParticipants(List<Participant> functionalRoleParticipants,
            List<string> totalListOfParticipants)
        {
            if (functionalRoleParticipants.Count > 0)
            {
                foreach (var participant in functionalRoleParticipants)
                {
                    totalListOfParticipants.Add(participant.FunctionalRoleCode);
                }
            }
        }

        protected async Task<List<Invitation>> GetInvitationsWithIncludesAsync(
            IReadOnlyContext context,
            List<int> invitationIds,
            CancellationToken cancellationToken)
            => await (from invitation in context.QuerySet<Invitation>()
                        .Include(i => i.Participants)
                        .Include(i => i.CommPkgs)
                        .Include(i => i.McPkgs)
                    where invitationIds.Contains(invitation.Id)
                    select invitation)
                .ToListAsync(cancellationToken);
    }
}
