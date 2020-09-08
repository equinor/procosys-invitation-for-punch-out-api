using System.Diagnostics;

namespace Equinor.ProCoSys.IPO.MainApi.Plant
{
    [DebuggerDisplay("{Title} ({Id})")]
    public class ProCoSysPlant
    {
        public string Id { get; set; }
        public string Title { get; set; }
    }
}
