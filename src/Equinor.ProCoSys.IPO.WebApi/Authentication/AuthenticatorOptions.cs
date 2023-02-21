using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Auth;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.WebApi.Authentication
{
    public class AuthenticatorOptions : IAuthenticatorOptions
    {
        protected readonly IOptionsMonitor<IpoAuthenticatorOptions> _options;

        private readonly IDictionary<string, string> _scopes = new Dictionary<string, string>();
        
        public AuthenticatorOptions(IOptionsMonitor<IpoAuthenticatorOptions> options)
        {
            _options = options;
            _scopes.Add("MainApiScope", _options.CurrentValue.MainApiScope);
            _scopes.Add("LibraryApiScope", _options.CurrentValue.LibraryApiScope);
        }

        public string Instance => _options.CurrentValue.Instance;

        public string ClientId => _options.CurrentValue.IpoApiClientId;

        public string Secret => _options.CurrentValue.IpoApiSecret;

        public Guid ObjectId => _options.CurrentValue.IpoApiObjectId;

        public IDictionary<string, string> Scopes => _scopes;
    }
}
