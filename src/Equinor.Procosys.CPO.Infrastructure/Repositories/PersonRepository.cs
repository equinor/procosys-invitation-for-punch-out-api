using System;
using System.Threading.Tasks;
using Equinor.Procosys.CPO.Domain.AggregateModels.PersonAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.Procosys.CPO.Infrastructure.Repositories
{
    public class PersonRepository : RepositoryBase<Person>, IPersonRepository
    {
        public PersonRepository(CPOContext context)
            : base(context, context.Persons) { }

        public Task<Person> GetByOidAsync(Guid oid)
            => DefaultQuery.SingleOrDefaultAsync(p => p.Oid == oid);
    }
}
