using System.Linq;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg
{
    public class ProCoSysCommPkg
    {
        public long Id { get; set; }
        public string CommPkgNo { get; set; }
        public string Description { get; set; }
        public string CommStatus { get; set; }
        public string SystemPath { get; set; }
        public string Section
            => SystemPath.Count(s => s == '|') == 2
                ? SystemPath.Substring(0, SystemPath.IndexOf('|'))
                : null;
    }
}
