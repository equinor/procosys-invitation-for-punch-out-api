using System;
using System.Collections.Generic;
using System.Text;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations
{
    public class ExternalEmailDto
    {
        public int? Id { get; set; }
        public string RowVersion { get; set; }
        public string Email { get; set; }
    }
}
