using System.Net.Http;
using Equinor.ProCoSys.Auth.Authentication;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Auth.Client
{
    /// <summary>
    /// Implementation of the abstract BearerTokenApiClient to access Main Api
    /// The implementation of IMainApiAuthenticator refer to the correct scope for Main Api
    /// </summary>
    public class MainApiClient : BearerTokenApiClient, IMainApiClient
    {
        public MainApiClient(IHttpClientFactory httpClientFactory,
            IMainApiAuthenticator mainApiAuthenticator,
            ILogger<MainApiClient> logger) : base(httpClientFactory, mainApiAuthenticator, logger)
        {
        }
    }
}
