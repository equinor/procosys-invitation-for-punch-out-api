using System;

namespace Equinor.Procosys.CPO.WebApi.Authentication
{
    public class AuthenticatorOptions
    {
        public string Instance { get; set; }

        public string CPOApiClientId { get; set; }
        public string CPOApiSecret { get; set; }

        public string MainApiClientId { get; set; }
        public string MainApiSecret { get; set; }
        public string MainApiScope { get; set; }
    }
}
