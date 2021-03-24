using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.Query.GetOutstandingIpos
{
    public class OutstandingIposResultDto
    {
        public OutstandingIposResultDto(
            int maxAvailable,
            IEnumerable<OutstandingIpoDetailsDto> items)
        {
            MaxAvailable = maxAvailable;
            Items = items;
        }
        public int MaxAvailable { get; }

        public IEnumerable<OutstandingIpoDetailsDto> Items { get; }
    }
}
