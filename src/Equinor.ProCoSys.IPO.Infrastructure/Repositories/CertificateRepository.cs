using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.CertificateAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories
{
    public class CertificateRepository : RepositoryBase<Certificate>, ICertificateRepository
    {
        public CertificateRepository(IPOContext context)
            : base(context, context.Certificates,
                  context.Certificates
                    .Include(x => x.CertificateCommPkgs)
                    .Include(x => x.CertificateMcPkgs))
        {
        }

        public Task<Certificate> GetCertificateByGuid(Guid certificateGuid)
            => DefaultQuery.SingleOrDefaultAsync(c => c.PcsGuid == certificateGuid);
    }
}
