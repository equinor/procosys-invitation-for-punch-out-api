using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class EditParticipantDto
    {
        public Organization Organization { get; set; }
        public int SortKey { get; set; }
        public EditExternalEmailDto ExternalEmail { get; set; }
        public EditInvitedPersonDto Person { get; set; }
        public EditFunctionalRoleDto FunctionalRole { get; set; }
    }
}
