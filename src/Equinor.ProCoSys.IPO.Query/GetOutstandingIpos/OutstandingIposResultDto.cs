using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.Query.GetOutstandingIpos
{
    public class OutstandingIposResultDto
    {
        public OutstandingIposResultDto(IEnumerable<OutstandingIpoDetailsDto> items) => Items = items;

        public IEnumerable<OutstandingIpoDetailsDto> Items { get; }
    }
}
