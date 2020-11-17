using System.Collections.Generic;
using Fusion.Integration.Meeting;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationById
{
    public class FunctionalRoleDto
    {
        public FunctionalRoleDto(
            string code,
            string email,
            IEnumerable<InvitedPersonDto> persons,
            string rowVersion)
        {
            Code = code;
            Email = email;
            Persons = persons;
            RowVersion = rowVersion;
        }

        public int Id { get; set; }
        public string Code { get; }
        public string Email { get; }
        public IEnumerable<InvitedPersonDto> Persons { get; }
        public OutlookResponse? Response { get; set; }
        public string RowVersion { get; }
    }
}
