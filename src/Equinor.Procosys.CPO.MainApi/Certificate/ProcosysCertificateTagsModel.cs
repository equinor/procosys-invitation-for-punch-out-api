using System.Collections.Generic;

namespace Equinor.Procosys.CPO.MainApi.Certificate
{
    public class ProcosysCertificateTagsModel
    {
        public bool CertificateIsAccepted { get; set; }
        public IEnumerable<ProcosysCertificateTag> Tags { get; set; }
    }
}
