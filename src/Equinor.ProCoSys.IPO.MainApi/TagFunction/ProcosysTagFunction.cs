using System.Diagnostics;

namespace Equinor.ProCoSys.IPO.MainApi.TagFunction
{
    [DebuggerDisplay("{Code}/{RegisterCode}")]
    public class ProCoSysTagFunction
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string RegisterCode { get; set; }
    }
}
