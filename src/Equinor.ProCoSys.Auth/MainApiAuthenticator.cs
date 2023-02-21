using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Auth
{
    public class MainApiAuthenticator : ApiAuthenticator, IMainApiTokenProvider
    {
        public MainApiAuthenticator(IOptionsMonitor<AuthenticatorOptions> options, ILogger<MainApiAuthenticator> logger)
            : base(options, logger)
        {
        }

        public async ValueTask<string> GetBearerTokenForMainApiOnBehalfOfCurrentUserAsync()
            => await GetBearerTokenOnBehalfOfCurrentUser(_options.CurrentValue.MainApiScope);

        public async ValueTask<string> GetBearerTokenForMainApiForApplicationAsync()
            => await GetBearerTokenForApplicationAsync(_options.CurrentValue.MainApiScope);
    }
}
