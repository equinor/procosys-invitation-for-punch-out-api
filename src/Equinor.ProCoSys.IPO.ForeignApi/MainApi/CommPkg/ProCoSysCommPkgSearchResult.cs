using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg
{
    public class ProCoSysCommPkgSearchResult
    {
        public int MaxAvailable { get; set; }
        public IList<ProCoSysSearchCommPkg> Items { get; set; }
    }
}
