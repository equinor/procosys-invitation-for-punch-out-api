using System;
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
        public string OperationHandoverStatus { get; set; }
        public Guid? RfocGuid { get; set; }
        public DateTime? M01 { get; set; }
        public DateTime? M02 { get; set; }
        public DateTime? RfocAcceptedAt { get; set; }
        public string System { get; set; }
        public Guid ProCoSysGuid { get; set; }
        public string Section
            => System.Count(s => s == '|') == 2
                ? System.Substring(0, System.IndexOf('|'))
                : null;
    }
}
