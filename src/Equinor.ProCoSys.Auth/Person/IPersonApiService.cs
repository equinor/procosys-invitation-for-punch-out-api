using System;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Auth.Person
{
    public interface IPersonApiService
    {
        Task<ProCoSysPerson> TryGetPersonByOidAsync(Guid azureOid);
    }
}
