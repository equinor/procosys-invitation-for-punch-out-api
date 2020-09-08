using System;

namespace Equinor.ProCoSys.IPO.Domain
{
    public interface ICurrentUserProvider
    {
        Guid GetCurrentUserOid();
    }
}
