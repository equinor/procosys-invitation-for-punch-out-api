using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace Equinor.ProCoSys.Auth
{
    public abstract class ApiAuthenticator : IBearerTokenSetter
    {
        protected readonly ILogger<ApiAuthenticator> _logger;

        private readonly IAuthenticatorOptions _options;
        private readonly string _secretInfo;
        private readonly string _apiScope;
        private string _requestToken;
        private readonly ConcurrentDictionary<string, string> _oboTokens = new ConcurrentDictionary<string, string>();
        private readonly ConcurrentDictionary<string, string> _appTokens = new ConcurrentDictionary<string, string>();

        public ApiAuthenticator(string apiScopeKey, IAuthenticatorOptions options, ILogger<ApiAuthenticator> logger)
        {
            var hashForDebug = this.GetHashCode();

            if (!options.Scopes.TryGetValue(apiScopeKey, out _apiScope))
            {
                throw new ArgumentException($"List of scopes dont have key {apiScopeKey}");
            }
            _options = options;
            _logger = logger;
            var secret = _options.Secret;
            _secretInfo = $"{secret.Substring(0, 2)}***{secret.Substring(secret.Length - 1, 1)}";
        }

        public AuthenticationType AuthenticationType { get; set; }

        public void SetBearerToken(string token) => _requestToken = token;

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
            if (!_oboTokens.ContainsKey(_apiScope))
            {
                if (string.IsNullOrEmpty(_requestToken))
                {
                    throw new ArgumentNullException(nameof(_requestToken));
                }

                var app = CreateConfidentialClient();

                var tokenResult = await app
                    .AcquireTokenOnBehalfOf(new List<string> { _apiScope },
                        new UserAssertion(_requestToken))
                    .ExecuteAsync();

                _oboTokens.TryAdd(_apiScope, tokenResult.AccessToken);
            }

            return _oboTokens[_apiScope];
        }

        private async ValueTask<string> GetBearerTokenForApplicationAsync()
        {
            if (!_appTokens.ContainsKey(_apiScope))
            {
                var app = CreateConfidentialClient();

                var tokenResult = await app
                    .AcquireTokenForClient(new List<string> { _apiScope })
                    .ExecuteAsync();

                _appTokens.TryAdd(_apiScope, tokenResult.AccessToken);
            }

            return _appTokens[_apiScope];
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
