using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Person;

namespace Equinor.ProCoSys.Auth.Caches
{
    // todo unit tests
    public interface IPersonCache
    {
        Task<bool> ExistsAsync(Guid userOid);
        Task<ProCoSysPerson> GetAsync(Guid userOid);
    }
}
