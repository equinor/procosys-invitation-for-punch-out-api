using System;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations.EditInvitation
{
    public class EditInvitedPersonDto
    {
        public Guid AzureOid { get; set; }
        public string Email { get; set; }
        public bool Required { get; set; }
        public int Id { get; set; }
        public string RowVersion { get; set; }
    }
}
