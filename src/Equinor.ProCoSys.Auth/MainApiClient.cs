using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Auth
{
    public class MainApiClient : BearerTokenApiClient, IMainApiClient
    {
        public MainApiClient(IHttpClientFactory httpClientFactory,
            IMainApiTokenProvider mainApiTokenProvider,
            ILogger<MainApiClient> logger) : base(httpClientFactory, mainApiTokenProvider, logger)
        {
        }
    }
}
