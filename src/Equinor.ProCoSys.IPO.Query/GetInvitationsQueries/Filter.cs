using System;
using System.Collections.Generic;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationsQueries
{
    public class Filter
    {
        public IList<IpoStatus> IpoStatuses { get; set; } = new List<IpoStatus>();
        public string FunctionalRoleCode { get; set; }
        public Guid? PersonOid { get; set; }
        public string IpoIdStartsWith { get; set; }
        public string CommPkgNoStartsWith { get; set; }
        public string McPkgNoStartsWith { get; set; }
        public string TitleStartsWith { get; set; }
        public DateTime? LastChangedAtFromUtc { get; set; }
        public DateTime? LastChangedAtToUtc { get; set; }
        public DateTime? PunchOutDateFromUtc { get; set; }
        public DateTime? PunchOutDateToUtc { get; set; }
        public IList<PunchOutDateFilterType> PunchOutDates { get; set; } = new List<PunchOutDateFilterType>();
        public string ProjectName { get; set; }
    }
}
