using Equinor.ProCoSys.Auth.Authentication;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.IPO.ForeignApi.Client
{
    /// <summary>
    /// Authenticator to access Library Api. Important that the implementation of IAuthenticatorOptions
    /// fill options.Scopes with value for key LibraryApiScopeKey
    /// </summary>
    public class LibraryApiAuthenticator : ApiAuthenticator, ILibraryApiAuthenticator
    {
        public static string LibraryApiScopeKey = "LibraryApiScope";

        public LibraryApiAuthenticator(IAuthenticatorOptions options, ILogger<LibraryApiAuthenticator> logger)
            : base(LibraryApiScopeKey, options, logger)
        {
        }
    }
}
