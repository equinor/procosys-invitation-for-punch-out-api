using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.MainApi.Certificate
{
    public interface ICertificateApiService
    {
        Task<ProCoSysCertificateTagsModel> TryGetCertificateTagsAsync(
            string plant, 
            string projectName,
            string certificateNo,
            string certificateType);
        
        Task<IEnumerable<ProCoSysCertificateModel>> GetAcceptedCertificatesAsync(
            string plant, 
            DateTime cutoffAcceptedTime);
    }
}
