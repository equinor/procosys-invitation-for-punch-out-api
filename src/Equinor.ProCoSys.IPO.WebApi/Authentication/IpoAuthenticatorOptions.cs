using System;

namespace Equinor.ProCoSys.IPO.WebApi.Authentication
{
    /// <summary>
    /// Options for Authentication. Read from application configuration via IOptionsMonitor.
    /// "Mapped" to the generic IAuthenticatorOptions
    /// </summary>
    public class IpoAuthenticatorOptions
    {
        public string Instance { get; set; }

        public string IpoApiClientId { get; set; }
        public string IpoApiSecret { get; set; }
        public Guid IpoApiObjectId { get; set; }

        public bool DisableProjectUserDataClaims { get; set; }
        public bool DisableRestrictionRoleUserDataClaims { get; set; }

        public string MainApiScope { get; set; }

        public string LibraryApiScope { get; set; }
    }
}
