using System;

namespace Equinor.ProCoSys.Auth
{
    public class AuthenticatorOptions
    {
        public string Instance { get; set; }

        public string IpoApiClientId { get; set; }
        public string IpoApiSecret { get; set; }
        public Guid IpoApiObjectId { get; set; }

        public string MainApiScope { get; set; }

        public string LibraryApiScope { get; set; }
    }
}
