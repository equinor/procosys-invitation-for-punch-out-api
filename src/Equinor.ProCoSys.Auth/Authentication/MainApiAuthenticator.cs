using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Auth.Authentication
{
    /// <summary>
    /// Authenticator to access Main Api. Important that the implementation of IAuthenticatorOptions
    /// fill options.Scopes with value for key MainApiScopeKey
    /// </summary>
    public class MainApiAuthenticator : ApiAuthenticator, IMainApiTokenProvider
    {
        public static string MainApiScopeKey = "MainApiScope";

        public MainApiAuthenticator(IAuthenticatorOptions options, ILogger<MainApiAuthenticator> logger)
            : base(MainApiScopeKey, options, logger)
        {
        }
    }
}
