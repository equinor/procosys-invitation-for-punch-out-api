using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.CertificateAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories
{
    public class CertificateRepository : RepositoryBase<Certificate>, ICertificateRepository
    {
        public CertificateRepository(IPOContext context)
            : base(context, context.Certificates)
        {
        }

        public Task<Certificate> GetCertificateByGuid(Guid certificateGuid) 
            => DefaultQuery.SingleOrDefaultAsync(c => c.PcsGuid == certificateGuid);

        async Task UpdateRfocStatusesAsync(Guid proCoSysGuid)
        {
            var certificate = await GetCertificateByGuid(proCoSysGuid);
            if (certificate == null)
            {
                // TODO
                return;
            }
            foreach (var commPkg in certificate.CertificateCommPkgs)
            {
                commPkg.RfocAccepted = false;
            }
            foreach (var mcPkg in certificate.CertificateMcPkgs)
            {
                mcPkg.RfocAccepted = false;
            }
        }

        
    }
}
