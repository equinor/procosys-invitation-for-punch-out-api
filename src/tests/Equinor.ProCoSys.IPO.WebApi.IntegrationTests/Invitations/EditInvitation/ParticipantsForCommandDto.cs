using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations.EditInvitation
{
    public class ParticipantsForCommandDto
    {
        public Organization Organization { get; set; }
        public int SortKey { get; set; }
        public ExternalEmailForCommandDto ExternalEmail { get; set; }
        public PersonForCommandDto Person { get; set; }
        public FunctionalRoleForCommandDto FunctionalRole { get; set; }
    }
}
