using System.Threading.Tasks;
using Equinor.ProCoSys.Auth;
using Equinor.ProCoSys.IPO.ForeignApi.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.WebApi.Authentication
{
    public class LibraryApiAuthenticator : ApiAuthenticator, ILibraryApiTokenProvider
    {
        public LibraryApiAuthenticator(IOptionsMonitor<AuthenticatorOptions> options, ILogger<LibraryApiAuthenticator> logger)
            : base(options, logger)
        {
        }

        public async ValueTask<string> GetBearerTokenForLibraryApiOnBehalfOfCurrentUserAsync()
            => await GetBearerTokenOnBehalfOfCurrentUser(_options.CurrentValue.LibraryApiScope);
    }
}
