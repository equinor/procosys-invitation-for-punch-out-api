using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories
{
    public class PersonRepository : RepositoryBase<Person>, IPersonRepository
    {
        public PersonRepository(IPOContext context)
            : base(context, context.Persons) { }

        public Task<Person> GetByOidAsync(Guid oid)
            => DefaultQuery.SingleOrDefaultAsync(p => p.Oid == oid);
    }
}
