using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.CertificateAggregate
{
    public interface ICertificateRepository : IRepository<Certificate>
    {
        Task<Certificate> GetCertificateByGuid(Guid pcsGuid);
        void UpdateRfocStatuses(Guid proCoSysGuid);
    }
}
