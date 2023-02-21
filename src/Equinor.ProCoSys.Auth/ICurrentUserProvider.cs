using System;

namespace Equinor.ProCoSys.Auth
{
    public interface ICurrentUserProvider
    {
        Guid GetCurrentUserOid();
    }
}
