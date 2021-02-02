using Fusion.Integration.Meeting;

namespace Equinor.ProCoSys.IPO.Query
{
    public class InvitedPersonDto
    {
        public InvitedPersonDto(PersonDto person) => Person = person;

        public PersonDto Person { get; }
        public OutlookResponse? Response { get; set; }
        public bool Required { get; set; }
    }
}
