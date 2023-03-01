using System.Net.Http;
using Equinor.ProCoSys.Auth.Client;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.IPO.ForeignApi.Client
{
    /// Implementation of the abstract BearerTokenApiClient to access Library Api
    /// The implementation of ILibraryApiAuthenticator refer to the correct scope for Library Api
    public class LibraryApiClient : BearerTokenApiClient, ILibraryApiClient
    {
        public LibraryApiClient(
            IHttpClientFactory httpClientFactory,
            ILibraryApiAuthenticator libraryApiAuthenticator,
            ILogger<LibraryApiClient> logger) : base(httpClientFactory, libraryApiAuthenticator, logger)
        {
        }
    }
}
