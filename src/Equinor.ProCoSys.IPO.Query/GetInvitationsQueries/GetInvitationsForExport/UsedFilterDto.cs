using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationsQueries.GetInvitationsForExport
{
    public class UsedFilterDto
    {
        public UsedFilterDto(
            string ipoIdStartsWith,
            string ipoTitleStartWith,
            string mcPkgNoStartsWith,
            string commPkgNoStartWith,
            IEnumerable<string> ipoStatuses,
            string functionalRoleInvited,
            string personInvited
            )
        {
            IpoIdStartsWith = ipoIdStartsWith;
            IpoTitleStartWith = ipoTitleStartWith;
            McPkgNoStartsWith = mcPkgNoStartsWith;
            CommPkgNoStartWith = commPkgNoStartWith;
            IpoStatuses = ipoStatuses;
            FunctionalRoleInvited = functionalRoleInvited;
            PersonInvited = personInvited;
        }
        public string IpoIdStartsWith { get; }
        public string IpoTitleStartWith { get; }
        public string McPkgNoStartsWith { get; }
        public string CommPkgNoStartWith { get; }
        public IEnumerable<string> IpoStatuses { get; }
        public string FunctionalRoleInvited { get; }
        public string PersonInvited { get; }
    }
}
