using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Auth.Authentication;
using Equinor.ProCoSys.IPO.ForeignApi.Client;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.WebApi.Authentication
{
    /// <summary>
    /// "Mapping" between application options read by IOptionsMonitor to generic IAuthenticatorOptions
    /// Needed because keys for configuration differ from application to application
    /// </summary>
    public class AuthenticatorOptions : IAuthenticatorOptions
    {
        protected readonly IOptionsMonitor<IpoAuthenticatorOptions> _options;

        private readonly IDictionary<string, string> _scopes = new Dictionary<string, string>();
        
        public AuthenticatorOptions(IOptionsMonitor<IpoAuthenticatorOptions> options)
        {
            _options = options;
            _scopes.Add(MainApiAuthenticator.MainApiScopeKey, _options.CurrentValue.MainApiScope);
            _scopes.Add(LibraryApiAuthenticator.LibraryApiScopeKey, _options.CurrentValue.LibraryApiScope);
        }

        public string Instance => _options.CurrentValue.Instance;

        public string ClientId => _options.CurrentValue.IpoApiClientId;

        public string Secret => _options.CurrentValue.IpoApiSecret;

        public Guid ObjectId => _options.CurrentValue.IpoApiObjectId;

        public bool DisableRestrictionRoleUserDataClaims
            => _options.CurrentValue.DisableRestrictionRoleUserDataClaims;

        public bool DisableProjectUserDataClaims
            => _options.CurrentValue.DisableProjectUserDataClaims;

        public IDictionary<string, string> Scopes => _scopes;
    }
}
