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
        protected readonly IAuthenticatorOptions _options;
        protected readonly ILogger<ApiAuthenticator> _logger;
        protected bool _canUseOnBehalfOf;
        protected readonly string _secretInfo;

        private string _requestToken;
        private readonly ConcurrentDictionary<string, string> _oboTokens = new ConcurrentDictionary<string, string>();
        private readonly ConcurrentDictionary<string, string> _appTokens = new ConcurrentDictionary<string, string>();

        public ApiAuthenticator(IAuthenticatorOptions options, ILogger<ApiAuthenticator> logger)
        {
            _options = options;
            _logger = logger;
            var secret = _options.Secret;
            _secretInfo = $"{secret.Substring(0, 2)}***{secret.Substring(secret.Length - 1, 1)}";
        }

        public void SetBearerToken(string token, bool isUserToken = true)
        {
            _requestToken = token ?? throw new ArgumentNullException(nameof(token));
            _canUseOnBehalfOf = isUserToken;
        }

        protected async ValueTask<string> GetBearerTokenOnBehalfOfCurrentUser(string apiScope)
        {
            if (_canUseOnBehalfOf)
            {
                if (!_oboTokens.ContainsKey(apiScope))
                {
                    var app = CreateConfidentialClient();

                    var tokenResult = await app
                        .AcquireTokenOnBehalfOf(new List<string> { apiScope },
                            new UserAssertion(_requestToken))
                        .ExecuteAsync();

                    _oboTokens.TryAdd(apiScope, tokenResult.AccessToken);
                }

                return _oboTokens[apiScope];
            }

            return _requestToken;
        }

        protected async ValueTask<string> GetBearerTokenForApplicationAsync(string apiScope)
        {
            if (!_appTokens.ContainsKey(apiScope))
            {
                var app = CreateConfidentialClient();

                var tokenResult = await app
                    .AcquireTokenForClient(new List<string> { apiScope })
                    .ExecuteAsync();

                _appTokens.TryAdd(apiScope, tokenResult.AccessToken);
            }

            return _appTokens[apiScope];
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
