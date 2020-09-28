using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.ForeignApi.Plant
{
    public interface IPlantCache
    {
        Task<IList<string>> GetPlantIdsForUserOidAsync(Guid userOid);
        Task<bool> IsValidPlantForUserAsync(string plantId, Guid userOid);
        Task<bool> IsValidPlantForCurrentUserAsync(string plantId);
        void Clear(Guid userOid);
    }
}
