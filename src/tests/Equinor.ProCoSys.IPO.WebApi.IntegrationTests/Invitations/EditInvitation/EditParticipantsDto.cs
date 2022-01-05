using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations.EditInvitation
{
    public class EditParticipantsDto
    {
        public Organization Organization { get; set; }
        public int SortKey { get; set; }
        public EditExternalEmailDto ExternalEmail { get; set; }
        public EditPersonDto Person { get; set; }
        public EditFunctionalRoleDto FunctionalRole { get; set; }
    }
}
