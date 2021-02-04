using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.Query.GetCommPkgsInProject
{
    public class ProCoSysCommPkgSearchDto
    {
        public ProCoSysCommPkgSearchDto(
            int maxAvailable,
            IList<ProCoSysCommPkgDto> commPkgs)
        {
            MaxAvailable = maxAvailable;
            CommPkgs = commPkgs;
        }

        public int MaxAvailable { get; }
        public IList<ProCoSysCommPkgDto> CommPkgs { get; }
    }
}
