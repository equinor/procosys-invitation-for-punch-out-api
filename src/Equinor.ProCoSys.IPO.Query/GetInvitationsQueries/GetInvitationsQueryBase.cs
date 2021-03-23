using System;
using System.Linq;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

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

        protected static IQueryable<InvitationForQueryDto> AddSorting(Sorting sorting, IQueryable<InvitationForQueryDto> queryable)
        {
            switch (sorting.Direction)
            {
                default:
                    switch (sorting.Property)
                    {
                        case SortingProperty.Type:
                            queryable = queryable.OrderBy(dto => dto.Type);
                            break;
                        case SortingProperty.Status:
                            queryable = queryable.OrderBy(dto => dto.Status);
                            break;
                        case SortingProperty.IpoNo:
                            queryable = queryable.OrderBy(dto => dto.Id);
                            break;
                        case SortingProperty.Title:
                            queryable = queryable.OrderBy(dto => dto.Title);
                            break;
                        case SortingProperty.PunchOutDateUtc:
                            queryable = queryable.OrderBy(dto => dto.StartTimeUtc);
                            break;
                        case SortingProperty.CompletedAtUtc:
                            queryable = queryable.OrderBy(dto => dto.CompletedAtUtc);
                            break;
                        case SortingProperty.AcceptedAtUtc:
                            queryable = queryable.OrderBy(dto => dto.AcceptedAtUtc);
                            break;
                        default:
                            queryable = queryable.OrderBy(dto => dto.Id);
                            break;
                    }

                    break;
                case SortingDirection.Desc:
                    switch (sorting.Property)
                    {
                        case SortingProperty.Type:
                            queryable = queryable.OrderByDescending(dto => dto.Type);
                            break;
                        case SortingProperty.Status:
                            queryable = queryable.OrderByDescending(dto => dto.Status);
                            break;
                        case SortingProperty.IpoNo:
                            queryable = queryable.OrderByDescending(dto => dto.Id);
                            break;
                        case SortingProperty.Title:
                            queryable = queryable.OrderByDescending(dto => dto.Title);
                            break;
                        case SortingProperty.PunchOutDateUtc:
                            queryable = queryable.OrderByDescending(dto => dto.StartTimeUtc);
                            break;
                        case SortingProperty.CompletedAtUtc:
                            queryable = queryable.OrderByDescending(dto => dto.CompletedAtUtc);
                            break;
                        case SortingProperty.AcceptedAtUtc:
                            queryable = queryable.OrderByDescending(dto => dto.AcceptedAtUtc);
                            break;
                        default:
                            queryable = queryable.OrderByDescending(dto => dto.Id);
                            break;
                    }
                    break;
            }

            return queryable;
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
    }
}
