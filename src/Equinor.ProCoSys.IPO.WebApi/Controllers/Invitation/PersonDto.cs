using System;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class PersonDto
    {
        public int? Id { get; set; }
        public Guid? AzureOid { get; set; }
        public string Email { get; set; }
        public bool Required { get; set; }
    }
}
