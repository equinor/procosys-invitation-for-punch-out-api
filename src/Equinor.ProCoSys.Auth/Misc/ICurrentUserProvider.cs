using System;

namespace Equinor.ProCoSys.Auth.Misc
{
    public interface ICurrentUserProvider
    {
        Guid GetCurrentUserOid();
        bool HasCurrentUser();
    }
}
