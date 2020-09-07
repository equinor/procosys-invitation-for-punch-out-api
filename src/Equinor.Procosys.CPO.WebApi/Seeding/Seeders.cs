using System;
using Equinor.Procosys.CPO.Domain.AggregateModels.PersonAggregate;

namespace Equinor.Procosys.CPO.WebApi.Seeding
{
    public static class Seeders
    {
        public static void AddUsers(this IPersonRepository personRepository, int entryCount)
        {
            for (var i = 0; i < entryCount; i++)
            {
                var user = new Person(Guid.NewGuid(), $"Firstname-{i}", $"Lastname-{i}");
                personRepository.Add(user);
            }
        }
    }
}
