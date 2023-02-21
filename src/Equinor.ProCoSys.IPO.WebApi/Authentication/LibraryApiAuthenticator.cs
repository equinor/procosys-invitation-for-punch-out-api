using System.Threading.Tasks;
using Equinor.ProCoSys.Auth;
using Equinor.ProCoSys.IPO.ForeignApi.Client;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.IPO.WebApi.Authentication
{
    public class LibraryApiAuthenticator : ApiAuthenticator, ILibraryApiTokenProvider
    {
        public LibraryApiAuthenticator(IAuthenticatorOptions options, ILogger<LibraryApiAuthenticator> logger)
            : base(options, logger)
        {
        }

        public async ValueTask<string> GetBearerTokenForLibraryApiOnBehalfOfCurrentUserAsync()
            => await GetBearerTokenOnBehalfOfCurrentUser(_options.Scopes["LibraryApiScope"]);
    }
}
