using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Auth
{
    public class MainApiAuthenticator : ApiAuthenticator, IMainApiTokenProvider
    {
        public MainApiAuthenticator(IAuthenticatorOptions options, ILogger<MainApiAuthenticator> logger)
            : base(options, logger)
        {
        }

        public async ValueTask<string> GetBearerTokenForMainApiOnBehalfOfCurrentUserAsync()
            => await GetBearerTokenOnBehalfOfCurrentUser(_options.Scopes["MainApiScope"]);

        public async ValueTask<string> GetBearerTokenForMainApiForApplicationAsync()
            => await GetBearerTokenForApplicationAsync(_options.Scopes["MainApiScope"]);
    }
}
