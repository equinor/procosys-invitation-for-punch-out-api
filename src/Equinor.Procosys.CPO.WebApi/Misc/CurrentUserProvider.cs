using System;
using Equinor.Procosys.CPO.Domain;

namespace Equinor.Procosys.CPO.WebApi.Misc
{
    public class CurrentUserProvider : ICurrentUserProvider, ICurrentUserSetter
    {
        private Guid? _currentUserOid;

        public Guid GetCurrentUserOid()
        {
            if (_currentUserOid.HasValue)
            {
                return _currentUserOid.Value;
            }

            throw new Exception("Unable to determine current user");
        }

        public void SetCurrentUser(Guid oid) => _currentUserOid = oid;
    }
}
