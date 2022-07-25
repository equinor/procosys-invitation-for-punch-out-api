using System.Linq;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg
{
    public class ProCoSysMcPkg
    {
        public long Id { get; set; }
        public string McPkgNo { get; set; }
        public string Description { get; set; }
        public string DisciplineCode { get; set; }
        public string CommPkgNo { get; set; }
        public string SystemPath { get; set; }
        public string Section
            => SystemPath.Count(s => s == '|') == 2
                ? SystemPath.Substring(0, SystemPath.IndexOf('|'))
                : null;
    }
}
