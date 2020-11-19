using System.Collections.Generic;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person
{
    public interface IPersonApiService
    {
        Task<IList<ProCoSysPerson>> GetPersonsAsync(string plant, string searchString, long numberOfRows = 1000);
        Task<IList<ProCoSysPerson>> GetPersonsByUserGroupAsync(string plant, string searchString, string userGroup);
        Task<IList<ProCoSysPerson>> GetPersonsByOidsAsync(string plant, IList<string> azureOids);
        Task<ProCoSysPerson> GetPersonByOidsInUserGroupAsync(string plant, string azureOid, string userGroup);
        Task<ProCoSysPerson> GetPersonInFunctionalRoleAsync(string plant, string azureOid, string functionalRoleCode);
    }
}
