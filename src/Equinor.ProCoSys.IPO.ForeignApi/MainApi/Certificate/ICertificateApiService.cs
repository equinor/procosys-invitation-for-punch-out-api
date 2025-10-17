using System;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.Certificate
{
    public interface ICertificateApiService
    {
        Task<PCSCertificateMcPkgsModel> TryGetCertificateMcPkgsAsync(string plant, Guid proCoSysGuid, CancellationToken cancellationToken);
        Task<PCSCertificateCommPkgsModel> TryGetCertificateCommPkgsAsync(string plant, Guid proCoSysGuid, CancellationToken cancellationToken);
    }
}
