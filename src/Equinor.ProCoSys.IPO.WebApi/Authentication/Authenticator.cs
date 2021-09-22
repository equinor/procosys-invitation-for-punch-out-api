using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.ForeignApi.Client;
using Equinor.ProCoSys.IPO.WebApi.Misc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace Equinor.ProCoSys.IPO.WebApi.Authentication
{
    public class Authenticator : IBearerTokenProvider, IBearerTokenSetter, IApplicationAuthenticator
    {
        private readonly IOptions<AuthenticatorOptions> _options;
        private readonly ILogger<Authenticator> _logger;
        private bool _canUseOnBehalfOf;
        private string _requestToken;
        private string _applicationToken;
        private readonly ConcurrentDictionary<string, string> _oboTokens = new ConcurrentDictionary<string, string>();
        private readonly string _secretInfo;

        public Authenticator(IOptions<AuthenticatorOptions> options, ILogger<Authenticator> logger)
        {
            _options = options;
            _logger = logger;
            var apiSecret = _options.Value.IPOApiSecret;
            _secretInfo = $"{apiSecret.Substring(0, 2)}***{apiSecret.Substring(apiSecret.Length - 1, 1)}";
        }

        public void SetBearerToken(string token, bool isUserToken = true)
        {
            _requestToken = token ?? throw new ArgumentNullException(nameof(token));
            _canUseOnBehalfOf = isUserToken;
        }

        public async ValueTask<string> GetBearerTokenForMainApiOnBehalfOfCurrentUserAsync()
            => await GetBearerTokenOnBehalfOfCurrentUser(_options.Value.MainApiScope);

        public async ValueTask<string> GetBearerTokenForLibraryApiOnBehalfOfCurrentUserAsync() 
            => await GetBearerTokenOnBehalfOfCurrentUser(_options.Value.LibraryApiScope);

        private async ValueTask<string> GetBearerTokenOnBehalfOfCurrentUser(string apiScope)
        {
            if (_canUseOnBehalfOf)
            {
                if (!_oboTokens.ContainsKey(apiScope))
                {
                    var app = CreateConfidentialIpoClient();

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

        public async ValueTask<string> GetBearerTokenForApplicationAsync()
        {
            if (_applicationToken == null)
            {
                var app = CreateConfidentialIpoClient();

                var tokenResult = await app
                    .AcquireTokenForClient(new List<string> { _options.Value.MainApiScope })
                    .ExecuteAsync();

                _applicationToken = tokenResult.AccessToken;
            }
            return _applicationToken;
        }

        private IConfidentialClientApplication CreateConfidentialIpoClient()
        {
            _logger.LogInformation($"Getting client using {_secretInfo} for {_options.Value.IPOApiClientId}");
            return ConfidentialClientApplicationBuilder
                .Create(_options.Value.IPOApiClientId)
                .WithClientSecret(_options.Value.IPOApiSecret)
                .WithAuthority(new Uri(_options.Value.Instance))
                .Build();
        }
    }
}
