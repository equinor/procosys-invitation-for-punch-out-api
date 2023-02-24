using System.Net.Http;
using Equinor.ProCoSys.Auth.Authentication;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Auth.Client
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
