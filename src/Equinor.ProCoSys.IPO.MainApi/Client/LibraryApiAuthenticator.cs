using Equinor.ProCoSys.Auth;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.IPO.ForeignApi.Client
{
    public class LibraryApiAuthenticator : ApiAuthenticator, ILibraryApiTokenProvider
    {
        public LibraryApiAuthenticator(IAuthenticatorOptions options, ILogger<LibraryApiAuthenticator> logger)
            : base("LibraryApiScope", options, logger)
        {
        }
    }
}
