using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationsQueries
{
    public abstract class GetInvitationsQueryBase
    {
        protected IQueryable<InvitationForQueryDto> CreateQueryableWithFilter(IReadOnlyContext context, string projectName, Filter filter, DateTime utcNow)
        {
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

            var queryable = from invitation in context.QuerySet<Invitation>()
                where invitation.ProjectName == projectName &&
                      (!filter.PunchOutDates.Any() ||
                           (filter.PunchOutDates.Contains(PunchOutDateFilterType.Overdue) && invitation.StartTimeUtc < utcNow) ||
                           (filter.PunchOutDates.Contains(PunchOutDateFilterType.ThisWeek) &&
                            invitation.StartTimeUtc >= startOfThisWeekUtc && invitation.StartTimeUtc < startOfNextWeekUtc) ||
                           (filter.PunchOutDates.Contains(PunchOutDateFilterType.NextWeek) &&
                            invitation.StartTimeUtc >= startOfNextWeekUtc && invitation.StartTimeUtc < startOfTwoWeeksUtc)) &&
                      (!filter.IpoStatuses.Any() ||
                            (filter.IpoStatuses.Contains(IpoStatus.Planned) && invitation.Status == IpoStatus.Planned) ||
                            (filter.IpoStatuses.Contains(IpoStatus.Completed) && invitation.Status == IpoStatus.Completed) ||
                            (filter.IpoStatuses.Contains(IpoStatus.Accepted) && invitation.Status == IpoStatus.Accepted) ||
                            (filter.IpoStatuses.Contains(IpoStatus.Canceled) && invitation.Status == IpoStatus.Canceled)) &&
                      (string.IsNullOrEmpty(filter.IpoIdStartsWith) ||
                            invitation.Id.ToString().StartsWith(ipoIdStartWith)) &&
                      (string.IsNullOrEmpty(filter.CommPkgNoStartsWith) ||
                            invitation.CommPkgs.Any(c => c.CommPkgNo.ToUpper().StartsWith(filter.CommPkgNoStartsWith.ToUpper())) ||
                            invitation.McPkgs.Any(mc => mc.CommPkgNo.ToUpper().StartsWith(filter.CommPkgNoStartsWith.ToUpper()))) &&
                      (string.IsNullOrEmpty(filter.McPkgNoStartsWith) ||
                            invitation.McPkgs.Any(mc => mc.McPkgNo.ToUpper().StartsWith(filter.McPkgNoStartsWith.ToUpper()))) &&
                      (string.IsNullOrEmpty(filter.TitleStartsWith) ||
                            invitation.Title.ToUpper().StartsWith(filter.TitleStartsWith.ToUpper())) &&
                      (filter.PersonOid == null ||
                            invitation.Participants.Any(p => p.AzureOid == filter.PersonOid)) &&
                      (filter.FunctionalRoleCode == null ||
                            invitation.Participants.Any(p => p.FunctionalRoleCode.ToUpper() == filter.FunctionalRoleCode.ToUpper())) &&
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
                    ProjectName = invitation.ProjectName,
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

        protected static IEnumerable<string> GetCommissioningReps(IEnumerable<Participant> participants)
        {
            var functionalRoleParticipants = participants.Where(p =>
                p.Organization == Organization.Commissioning && p.Type == IpoParticipantType.FunctionalRole).ToList();

            IList<string> participantFunctionalRoleCodes = new List<string>();
            if (functionalRoleParticipants.Count > 0)
            {
                foreach (var participant in functionalRoleParticipants)
                {
                    participantFunctionalRoleCodes.Add(participant.FunctionalRoleCode);
                }
                return participantFunctionalRoleCodes;
            }

            var personParticipants = participants.Where(p => p.Organization == Organization.Commissioning).ToList();
            IList<string> personParticipantNames = new List<string>();
            if (personParticipants.Count > 0)
            {
                foreach (var participantName in personParticipants.Select(NameCombiner))
                {
                    personParticipantNames.Add(participantName);

                }
            }
            return personParticipantNames;
        }

        protected static IEnumerable<string> GetOperationReps(IList<Participant> participants)
        {
            var functionalRoleParticipants =
                participants.Where(p => p.Organization == Organization.Operation 
                                        && p.Type == IpoParticipantType.FunctionalRole).ToList();

            IList<string> participantFunctionalRoleCodes = new List<string>();
            if (functionalRoleParticipants.Count > 0)
            {
                foreach (var participant in functionalRoleParticipants)
                {
                    participantFunctionalRoleCodes.Add(participant.FunctionalRoleCode);
                }
                return participantFunctionalRoleCodes;
            }

            var personParticipants = participants.Where(p => p.Organization == Organization.Operation).ToList();

            IList<string> personParticipantNames = new List<string>();
            if (personParticipants.Count > 0)
            {
                foreach (var participantName in personParticipants.Select(NameCombiner))
                {
                    personParticipantNames.Add(participantName);
                }
            }
            return personParticipantNames;
        }

        protected static IEnumerable<string> GetTechnicalIntegrityReps(IList<Participant> participants)
        {
            var functionalRoleParticipants = participants.Where(p =>
                    p.Organization == Organization.TechnicalIntegrity 
                    && p.Type == IpoParticipantType.FunctionalRole)
                .ToList();

            IList<string> participantFunctionalRoleCodes = new List<string>();
            if (functionalRoleParticipants.Count > 0)
            {
                foreach (var participant in functionalRoleParticipants)
                {
                    participantFunctionalRoleCodes.Add(participant.FunctionalRoleCode);
                }

                return participantFunctionalRoleCodes;
            }

            var personParticipants = participants.Where(p => p.Organization == Organization.TechnicalIntegrity).ToList();

            IList<string> personParticipantNames = new List<string>();
            if (personParticipants.Count > 0)
            {
                foreach (var participantName in personParticipants.Select(NameCombiner))
                {
                    personParticipantNames.Add(participantName);
                }
            }
            return personParticipantNames;
        }

        protected static IEnumerable<string> GetSupplierReps(IList<Participant> participants)
        {
            var functionalRoleParticipants =
                participants.Where(p => p.Organization == Organization.Supplier 
                                        && p.Type == IpoParticipantType.FunctionalRole).ToList();


            IList<string> participantFunctionalRoleCodes = new List<string>();
            if (functionalRoleParticipants.Count > 0)
            {
                foreach (var participant in functionalRoleParticipants)
                {
                    participantFunctionalRoleCodes.Add(participant.FunctionalRoleCode);
                }

                return participantFunctionalRoleCodes;
            }

            var personParticipants = participants.Where(p => p.Organization == Organization.Supplier).ToList();

            IList<string> personParticipantNames = new List<string>();
            if (personParticipants.Count > 0)
            {
                foreach (var participantName in personParticipants.Select(NameCombiner))
                {
                    personParticipantNames.Add(participantName);
                }
            }
            return personParticipantNames;
        }

        protected static IEnumerable<string> GetExternalGuests(IList<Participant> participants)
        {
            var externalGuestParticipants = participants.Where(p => p.Organization == Organization.External).ToList();

            var externalGuestParticipantEmails = new List<string>();
            foreach (var participant in externalGuestParticipants)
            {
                externalGuestParticipantEmails.Add(participant.Email);
            }

            return externalGuestParticipantEmails;
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
