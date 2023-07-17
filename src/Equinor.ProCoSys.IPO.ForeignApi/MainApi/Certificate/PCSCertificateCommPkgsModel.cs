using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.Certificate
{
    public class PCSCertificateCommPkgsModel
    {
        public bool CertificateIsAccepted { get; set; }
        public IEnumerable<PCSCertificateCommPkg> CommPkgs { get; set; }
    }
}
