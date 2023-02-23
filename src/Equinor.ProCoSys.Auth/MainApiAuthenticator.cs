using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Auth
{
    public class MainApiAuthenticator : ApiAuthenticator, IMainApiTokenProvider
    {
        public MainApiAuthenticator(IAuthenticatorOptions options, ILogger<MainApiAuthenticator> logger)
            : base("MainApiScope", options, logger)
        {
        }
    }
}
