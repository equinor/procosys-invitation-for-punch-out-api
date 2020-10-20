using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class ParticipantDto
    {
        public Organization Organization { get; set; }
        public int SortKey { get; set; }
        public string ExternalEmail { get; set; }
        public PersonDto Person { get; set; }
        public FunctionalRoleDto FunctionalRole { get; set; }
    }
}
