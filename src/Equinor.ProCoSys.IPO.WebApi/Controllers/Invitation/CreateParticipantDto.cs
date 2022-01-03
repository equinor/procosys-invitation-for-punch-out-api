using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class CreateParticipantDto
    {
        public Organization Organization { get; set; }
        public int SortKey { get; set; }
        public CreateExternalEmailDto ExternalEmail { get; set; }
        public CreateInvitedPersonDto Person { get; set; }
        public CreateFunctionalRoleDto FunctionalRole { get; set; }
    }
}
