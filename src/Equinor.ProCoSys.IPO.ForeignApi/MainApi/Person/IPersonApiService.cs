using System.Collections.Generic;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person
{
    public interface IPersonApiService
    {
        Task<IList<ProCoSysPerson>> GetPersonsAsync(string plant, string searchString, long numberOfRows = 1000);
        Task<IList<ProCoSysPerson>> GetPersonsWithPrivilegesAsync(string plant, string searchString, string objectName, IList<string> privileges);
        Task<IList<ProCoSysPerson>> GetPersonsByOidsAsync(string plant, IList<string> azureOids);
        Task<ProCoSysPerson> GetPersonByOidWithPrivilegesAsync(string plant, string azureOid, string objectName, IList<string> privileges);
        Task<ProCoSysPerson> GetPersonInFunctionalRoleAsync(string plant, string azureOid, string functionalRoleCode);
    }
}
