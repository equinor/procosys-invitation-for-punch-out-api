using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations.CreateInvitation
{
    public class CreateParticipantsDto
    {
        public Organization Organization { get; set; }
        public int SortKey { get; set; }
        public CreateExternalEmailForDto ExternalEmail { get; set; }
        public CreatePersonDto Person { get; set; }
        public CreateFunctionalRoleForDto FunctionalRole { get; set; }
    }
}
