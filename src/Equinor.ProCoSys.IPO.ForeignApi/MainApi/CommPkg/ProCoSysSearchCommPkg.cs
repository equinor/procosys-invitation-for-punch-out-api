using System.Linq;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg
{
    public class ProCoSysSearchCommPkg
    {
        public long Id { get; set; }
        public string CommPkgNo { get; set; }
        public string Description { get; set; }
        public string CommStatus { get; set; }
        public string OperationHandoverStatus { get; set; }
        public string System { get; set; }
        public string Section
            => System.Count(s => s == '|') == 2
                ? System.Substring(0, System.IndexOf('|'))
                : null;
    }
}
