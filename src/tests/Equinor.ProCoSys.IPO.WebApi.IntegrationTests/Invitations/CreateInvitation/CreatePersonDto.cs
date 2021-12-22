using System;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations.CreateInvitation
{
    public class CreatePersonDto
    {
        public Guid AzureOid { get; set; }
        public string Email { get; set; }
        public bool Required { get; set; }
    }
}
