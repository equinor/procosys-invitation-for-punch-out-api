using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.MainApi.Certificate
{
    public class ProCoSysCertificateTagsModel
    {
        public bool CertificateIsAccepted { get; set; }
        public IEnumerable<ProCoSysCertificateTag> Tags { get; set; }
    }
}
