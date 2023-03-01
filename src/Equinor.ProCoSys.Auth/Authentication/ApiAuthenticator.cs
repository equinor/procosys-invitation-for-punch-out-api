using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace Equinor.ProCoSys.Auth.Authentication
{
    /// <summary>
    /// Abstract class to be used for acquire a token to authentice a HttpClient to access a 
    /// foreign API. The authentication can be done in two ways via AuthenticationType:
    ///  * on behalf of logged on user using the OAuth 2.0 On-Behalf-Of flow
    ///  * as the client itself (in the name of no user) using the client credentials flow
    /// </summary>
    public abstract class ApiAuthenticator : IBearerTokenSetter
    {
        protected readonly ILogger<ApiAuthenticator> _logger;

        private readonly IAuthenticatorOptions _options;
        private readonly string _secretInfo;
        private readonly string _apiScope;
        private string _bearerTokenFromRequest;

        /// <summary>
        /// cache created OnBehalfOf tokens during this request
        /// </summary>
        private readonly ConcurrentDictionary<string, string> _oboTokenCache
            = new ConcurrentDictionary<string, string>();
        /// <summary>
        /// cache created application client tokens during this request
        /// </summary>
        private readonly ConcurrentDictionary<string, string> _appTokenCache
            = new ConcurrentDictionary<string, string>();

        public ApiAuthenticator(string apiScopeKey, IAuthenticatorOptions options, ILogger<ApiAuthenticator> logger)
        {
            if (!options.Scopes.TryGetValue(apiScopeKey, out _apiScope))
            {
                throw new ArgumentException(
                    $"List of scopes in {nameof(IAuthenticatorOptions)} dont have key {apiScopeKey}. " + 
                    $"Check {nameof(IAuthenticatorOptions)} implementation when filling {nameof(options.Scopes)} from the application config");
            }
            _options = options;
            _logger = logger;
            var secret = _options.Secret;
            _secretInfo = $"{secret.Substring(0, 2)}***{secret.Substring(secret.Length - 1, 1)}";
            AuthenticationType = AuthenticationType.OnBehalfOf;
        }

        public AuthenticationType AuthenticationType { get; set; }

        public void SetBearerToken(string bearerToken) => _bearerTokenFromRequest = bearerToken;

        public async ValueTask<string> GetBearerTokenAsync()
        {
            switch (AuthenticationType)
            {
                case AuthenticationType.OnBehalfOf:
                    return await GetBearerTokenOnBehalfOfCurrentUserAsync();
                case AuthenticationType.AsApplication:
                    return await GetBearerTokenForApplicationAsync();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async ValueTask<string> GetBearerTokenOnBehalfOfCurrentUserAsync()
        {
            if (!_oboTokenCache.ContainsKey(_apiScope))
            {
                if (string.IsNullOrEmpty(_bearerTokenFromRequest))
                {
                    throw new ArgumentNullException(nameof(_bearerTokenFromRequest));
                }

                var app = CreateConfidentialClient();

                var tokenResult = await app
                    .AcquireTokenOnBehalfOf(new List<string> { _apiScope },
                        new UserAssertion(_bearerTokenFromRequest))
                    .ExecuteAsync();

                _oboTokenCache.TryAdd(_apiScope, tokenResult.AccessToken);
            }

            return _oboTokenCache[_apiScope];
        }

        private async ValueTask<string> GetBearerTokenForApplicationAsync()
        {
            if (!_appTokenCache.ContainsKey(_apiScope))
            {
                var app = CreateConfidentialClient();

                var tokenResult = await app
                    .AcquireTokenForClient(new List<string> { _apiScope })
                    .ExecuteAsync();

                _appTokenCache.TryAdd(_apiScope, tokenResult.AccessToken);
            }

            return _appTokenCache[_apiScope];
        }

        private IConfidentialClientApplication CreateConfidentialClient()
        {
            _logger.LogInformation($"Getting client using {_secretInfo} for {_options.ClientId}");
            return ConfidentialClientApplicationBuilder
                .Create(_options.ClientId)
                .WithClientSecret(_options.Secret)
                .WithAuthority(new Uri(_options.Instance))
                .Build();
        }
    }
}
