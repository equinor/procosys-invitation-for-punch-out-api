using System;

namespace Equinor.Procosys.CPO.Domain
{
    public interface ICurrentUserProvider
    {
        Guid GetCurrentUserOid();
    }
}
