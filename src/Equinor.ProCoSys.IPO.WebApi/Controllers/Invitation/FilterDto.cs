using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Query.GetInvitationsQueries;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class FilterDto
    {
        public string ProjectName { get; set; }
        public IEnumerable<IpoStatus> IpoStatuses { get; set; }
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
        public IEnumerable<PunchOutDateFilterType> PunchOutDates { get; set; }
    }
}
