using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.Certificate
{
    public class PCSCertificateMcPkgsModel
    {
        public bool CertificateIsAccepted { get; set; }
        public IEnumerable<PCSCertificateMcPkg> McPkgs { get; set; }
    }
}
