using System.Net.Http;
using Equinor.ProCoSys.Auth.Client;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.IPO.ForeignApi.Client
{
    public class LibraryApiClient : BearerTokenApiClient, ILibraryApiClient
    {
        public LibraryApiClient(
            IHttpClientFactory httpClientFactory,
            ILibraryApiTokenProvider libraryApiTokenProvider,
            ILogger<LibraryApiClient> logger) : base(httpClientFactory, libraryApiTokenProvider, logger)
        {
        }
    }
}
