using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.MainApi.Tag
{
    public class ProCoSysTagDetailsComparer : IEqualityComparer<ProCoSysTagDetails>
    {
        public bool Equals(ProCoSysTagDetails d1, ProCoSysTagDetails d2)
        {
            if (d2 == null && d1 == null)
            {
                return true;
            }

            if (d1 == null || d2 == null)
            {
                return false;
            }

            if (d1.RegisterCode == d2.RegisterCode && d1.TagFunctionCode == d2.TagFunctionCode)
            {
                return true;
            }
                
            return false;
        }

        public int GetHashCode(ProCoSysTagDetails d)
        {
            var hCode = d.RegisterCode.GetHashCode() ^ d.TagFunctionCode.GetHashCode();
            return hCode.GetHashCode();
        }
    }
}
