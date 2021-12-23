using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Me
{
    public class OutstandingIposResultDto
    {
        public IEnumerable<OutstandingIpoDetailsDto> Items { get; set; }
    }
}
