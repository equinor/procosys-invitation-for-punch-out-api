using System;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class EditInvitedPersonDto
    {
        public int Id { get; set; }
        public Guid? AzureOid { get; set; }
        public string Email { get; set; }
        public bool Required { get; set; }
        public string RowVersion { get; set; }
    }
}
