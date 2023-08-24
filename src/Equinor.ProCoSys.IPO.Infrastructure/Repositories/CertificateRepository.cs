using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.CertificateAggregate;

namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories
{
    public class CertificateRepository : RepositoryBase<Certificate>, ICertificateRepository
    {
        public CertificateRepository(IPOContext context)
            : base(context, context.Certificates)
        {
        }

        public IList<Certificate> GetCertificateByGuid(Guid certificateGuid) 
            => _context.Certificates.Where(c => c.PcsGuid == certificateGuid).ToList();

        public IList<Certificate> GetMcPkgIdsByCertificateGuid(Guid certificateGuid)
            => _context.Certificates.Where(c => c.PcsGuid == certificateGuid).ToList();
    }
}
