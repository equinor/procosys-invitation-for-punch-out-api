using System.Collections.Generic;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person
{
    public interface IPersonApiService
    {
        Task<IList<ProCoSysPerson>> GetPersonsByUserGroupAsync(string plant, string searchString, string privilege);
    }
}
