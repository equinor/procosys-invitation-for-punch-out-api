using System;

namespace Equinor.ProCoSys.Auth.Misc
{
    public interface ICurrentUserSetter
    {
        void SetCurrentUserOid(Guid oid);
    }
}
