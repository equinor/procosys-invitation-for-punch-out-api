using System;
using System.Linq;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.IPO.Query.GetInvitations
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

            // No .Include() here. EF do not support .Include together with selecting a projection (dto).
            // If the select-statement select tag so queryable has been of type IQueryable<Tag>, .Include(t => t.Requirements) work fine
            var queryable = from invitation in context.QuerySet<Invitation>()
                    .Include(i => i.CommPkgs)
                    .Include(i => i.McPkgs)
                    .Include(i => i.Participants)
                            where invitation.ProjectName == projectName &&
                                  (!filter.PunchOutDates.Any() ||
                                       (filter.PunchOutDates.Contains(PunchOutDateFilterType.Overdue) && invitation.StartTimeUtc < startOfThisWeekUtc) ||
                                       (filter.PunchOutDates.Contains(PunchOutDateFilterType.ThisWeek) && invitation.StartTimeUtc >= startOfThisWeekUtc && invitation.StartTimeUtc < startOfNextWeekUtc) ||
                                       (filter.PunchOutDates.Contains(PunchOutDateFilterType.NextWeek) && invitation.StartTimeUtc >= startOfNextWeekUtc && invitation.StartTimeUtc < startOfTwoWeeksUtc)) &&
                                  (!filter.IpoStatuses.Any() ||
                                        (filter.IpoStatuses.Contains(IpoStatus.Planned) ||
                                        (filter.IpoStatuses.Contains(IpoStatus.Completed) ||
                                        (filter.IpoStatuses.Contains(IpoStatus.Accepted) ||
                                        (filter.IpoStatuses.Contains(IpoStatus.Canceled)) &&
                                  (string.IsNullOrEmpty(filter.IpoIdStartsWith) ||
                                        invitation.Id.ToString().StartsWith(ipoIdStartWith)) &&
                                  (string.IsNullOrEmpty(filter.CommPkgNoStartsWith) ||
                                        invitation.CommPkgs.Any(c => c.CommPkgNo.StartsWith(filter.CommPkgNoStartsWith) ||
                                        invitation.McPkgs.Any(mc => mc.CommPkgNo.StartsWith(filter.CommPkgNoStartsWith)) && 
                                  (string.IsNullOrEmpty(filter.McPkgNoStartsWith) ||
                                         invitation.McPkgs.Any(mc => mc.McPkgNo.StartsWith(filter.McPkgNoStartsWith)) &&
                                  (string.IsNullOrEmpty(filter.TitleStartsWith) || 
                                         invitation.Title.StartsWith(filter.TitleStartsWith)) &&
                                         (filter.PunchOutDateFromUtc != null ||
                                          invitation.StartTimeUtc >= filter.PunchOutDateFromUtc) &&
                                         (filter.PunchOutDateToUtc != null ||
                                          invitation.EndTimeUtc <= filter.PunchOutDateToUtc) &&
                                         (filter.LastChangedAtFromUtc != null ||
                                          invitation.ModifiedAtUtc >= filter.LastChangedAtFromUtc) &&
                                         (filter.LastChangedAtToUtc != null ||
                                          invitation.ModifiedAtUtc <= filter.LastChangedAtToUtc)
                            select new InvitationForQueryDto
                            {
                                IpoId = invitation.Id,
                                Title = invitation.Title,
                                Description = invitation.Description,
                                Status = invitation.Status,
                                Type = invitation.Type,
                                CreatedAtUtc = invitation.CreatedAtUtc,
                                CompletedAtUtc = invitation.CompletedAtUtc,
                                AcceptedAtUtc = invitation.AcceptedAtUtc,
                                RowVersion = invitation.RowVersion
                            };
            return queryable;
        }

        protected IQueryable<InvitationForQueryDto> AddSorting(Sorting sorting, IQueryable<InvitationForQueryDto> queryable)
        {
            switch (sorting.Direction)
            {
                default:
                    switch (sorting.Property)
                    {
                        case SortingProperty.Status:
                            queryable = queryable.OrderBy(dto => dto.Status);
                            break;
                        case SortingProperty.IpoNo:
                            queryable = queryable.OrderBy(dto => dto.IpoId);
                            break;
                        case SortingProperty.Title:
                            queryable = queryable.OrderBy(dto => dto.Title);
                            break;
                        case SortingProperty.PunchOutDateUtc:
                            queryable = queryable.OrderBy(dto => dto.CreatedAtUtc);
                            break;
                        case SortingProperty.CompletedAtUtc:
                            queryable = queryable.OrderBy(dto => dto.CompletedAtUtc);
                            break;
                        case SortingProperty.AcceptedAtUtc:
                            queryable = queryable.OrderBy(dto => dto.AcceptedAtUtc);
                            break;
                        default:
                            queryable = queryable.OrderBy(dto => dto.IpoId);
                            break;
                    }

                    break;
                case SortingDirection.Desc:
                    switch (sorting.Property)
                    {
                        case SortingProperty.Status:
                            queryable = queryable.OrderByDescending(dto => dto.Status);
                            break;
                        case SortingProperty.IpoNo:
                            queryable = queryable.OrderByDescending(dto => dto.IpoId);
                            break;
                        case SortingProperty.Title:
                            queryable = queryable.OrderByDescending(dto => dto.Title);
                            break;
                        case SortingProperty.PunchOutDateUtc:
                            queryable = queryable.OrderByDescending(dto => dto.CreatedAtUtc);
                            break;
                        case SortingProperty.CompletedAtUtc:
                            queryable = queryable.OrderByDescending(dto => dto.CompletedAtUtc);
                            break;
                        case SortingProperty.AcceptedAtUtc:
                            queryable = queryable.OrderByDescending(dto => dto.AcceptedAtUtc);
                            break;
                        default:
                            queryable = queryable.OrderByDescending(dto => dto.IpoId);
                            break;
                    }
                    break;
            }

            return queryable;
        }

        private string GetIpoIdStartWith(string filterString)
        {
            try
            {
                return int.Parse(filterString).ToString();
            }
            catch
            {
                if (filterString.Substring(0, 4).ToUpper() == "IPO-")
                {
                    return filterString.Substring(4);
                }

                return null;
            }
        }
    }
}
