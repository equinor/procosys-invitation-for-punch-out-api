using System;

namespace Equinor.ProCoSys.IPO.WebApi.Misc
{
    public interface ICurrentUserSetter
    {
        void SetCurrentUser(Guid oid);
    }
}
