using System;

namespace Equinor.ProCoSys.Auth
{
    public class AuthenticatorOptions2
    {
        public string Instance { get; private set; }

        public string ClientId { get; private set; }
        public string Secret { get; private set; }
        public Guid ObjectId { get; private set; }
        public string MainApiScope { get; private set; }
        public string MainApiVersion { get; private set; }
        public string MainApiBaseAddress { get; private set; }

        public AuthenticatorOptions2 UseInstance(string instance)
        {
            Instance = instance;
            return this;
        }

        public AuthenticatorOptions2 UseClientId(string clientId)
        {
            ClientId = clientId;
            return this;
        }

        public AuthenticatorOptions2 UseSecret(string secret)
        {
            Secret = secret;
            return this;
        }

        public AuthenticatorOptions2 UseObjectId(string objectId)
        {
            if (!Guid.TryParse(objectId, out Guid ObjectId))
            {
                throw new ArgumentException("Invalid objectId. Not a Guid");
            }
            return this;
        }

        public AuthenticatorOptions2 UseMainApiScope(string mainApiScope)
        {
            MainApiScope = mainApiScope;
            return this;
        }

        public AuthenticatorOptions2 UseMainApiVersion(string mainApiVersion)
        {
            MainApiVersion = mainApiVersion;
            return this;
        }

        public AuthenticatorOptions2 UseMainApiBaseAddress(string mainApiBaseAddress)
        {
            MainApiBaseAddress = mainApiBaseAddress;
            return this;
        }

    }
}
