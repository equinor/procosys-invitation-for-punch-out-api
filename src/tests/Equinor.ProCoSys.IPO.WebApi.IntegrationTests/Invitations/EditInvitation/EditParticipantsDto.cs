using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations.EditInvitation
{
    public class EditParticipantsDto
    {
        public Organization Organization { get; set; }
        public int SortKey { get; set; }
        public EditExternalEmailForDto ExternalEmail { get; set; }
        public EditPersonDto Person { get; set; }
        public EditFunctionalRoleForDto FunctionalRole { get; set; }
    }
}
