using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.Extensions.Configuration;

namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories.RawSql
{
    public class InvitationRawSqlRepository : RawSqlRepositoryBase
    {
        public InvitationRawSqlRepository(IConfiguration configuration) : base(configuration)
        {

        }

        public async Task<IEnumerable<OutstandingIpoDto>>
    }

    public class OutstandingIpoDto
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public Organization Organization { get; set; }
    }
}
