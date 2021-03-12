using System;
using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationsQueries.GetInvitationsForExport
{
    public class UsedFilterDto
    {
        public UsedFilterDto(
            string plant,
            string projectName,
            string ipoIdStartsWith,
            string ipoTitleStartWith,
            string mcPkgNoStartsWith,
            string commPkgNoStartWith,
            IEnumerable<string> ipoStatuses,
            DateTime? punchOutDateFromUtc,
            DateTime? punchOutDateToUtc,
            DateTime? lastChangedFromUtc,
            DateTime? lastChangedToUtc,
            string functionalRoleInvited,
            string personInvited)
        {
            Plant = plant;
            ProjectName = projectName;
            IpoIdStartsWith = ipoIdStartsWith;
            IpoTitleStartWith = ipoTitleStartWith;
            McPkgNoStartsWith = mcPkgNoStartsWith;
            CommPkgNoStartWith = commPkgNoStartWith;
            IpoStatuses = ipoStatuses;
            PunchOutDateFromUtc = punchOutDateFromUtc;
            PunchOutDateToUtc = punchOutDateToUtc;
            LastChangedFromUtc = lastChangedFromUtc;
            LastChangedToUtc = lastChangedToUtc;
            FunctionalRoleInvited = functionalRoleInvited;
            PersonInvited = personInvited;
        }

        public string Plant { get; }
        public string ProjectName { get; }
        public string IpoIdStartsWith { get; }
        public string IpoTitleStartWith { get; }
        public string McPkgNoStartsWith { get; }
        public string CommPkgNoStartWith { get; }
        public IEnumerable<string> IpoStatuses { get; }
        public DateTime? PunchOutDateFromUtc { get; }
        public DateTime? PunchOutDateToUtc { get; }
        public DateTime? LastChangedFromUtc { get; }
        public DateTime? LastChangedToUtc { get; }
        public string FunctionalRoleInvited { get; }
        public string PersonInvited { get; }
    }
}
