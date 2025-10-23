using Azure.Storage.Blobs.Models;

namespace Equinor.ProCoSys.IPO.Query;

public interface IQueryUserDelegationProvider
{
    public UserDelegationKey GetUserDelegationKey();
}
