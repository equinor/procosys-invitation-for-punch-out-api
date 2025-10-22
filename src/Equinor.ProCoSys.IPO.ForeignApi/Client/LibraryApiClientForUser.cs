using System.Net.Http;
using Equinor.ProCoSys.Auth.Client;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.IPO.ForeignApi.Client
{
    public class LibraryApiClientForUser(
        IHttpClientFactory httpClientFactory,
        ILogger<LibraryApiClientForUser> logger)
        : BearerTokenApiClient(ClientName, httpClientFactory, logger), ILibraryApiForUserClient
    {
        public const string ClientName = "LibraryApiClientForUser";
    }
}
