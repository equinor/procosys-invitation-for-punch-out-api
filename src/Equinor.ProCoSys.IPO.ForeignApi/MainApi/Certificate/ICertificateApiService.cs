using System;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.Certificate
{
    public interface ICertificateApiService
    {
        Task<PCSCertificateMcPkgsModel> GetCertificateMcPkgsAsync(string plant, Guid proCoSysGuid);
        Task<PCSCertificateCommPkgsModel> GetCertificateCommPkgsAsync(string plant, Guid proCoSysGuid);
    }
}
