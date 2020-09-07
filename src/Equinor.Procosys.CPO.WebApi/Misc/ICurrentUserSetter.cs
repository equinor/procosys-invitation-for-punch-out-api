using System;

namespace Equinor.Procosys.CPO.WebApi.Misc
{
    public interface ICurrentUserSetter
    {
        void SetCurrentUser(Guid oid);
    }
}
