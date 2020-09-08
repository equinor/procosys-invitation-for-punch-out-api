using System.Collections.Generic;
using System.Diagnostics;

namespace Equinor.ProCoSys.IPO.MainApi.Tag
{
    [DebuggerDisplay("{Items.Count} of {MaxAvailable} available tags")]
    public class ProCoSysTagSearchResult
    {
        public int MaxAvailable { get; set; }
        public List<ProCoSysTagOverview> Items { get; set; }
    }
}
