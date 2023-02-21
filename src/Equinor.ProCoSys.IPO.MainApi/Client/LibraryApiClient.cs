using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.IPO.ForeignApi.Client
{
    public class LibraryApiClient : BearerTokenApiClient, ILibraryApiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILibraryApiTokenProvider _libraryApiTokenProvider;

        public LibraryApiClient(IHttpClientFactory httpClientFactory,
            ILibraryApiTokenProvider libraryApiTokenProvider,
            ILogger<LibraryApiClient> logger) : base(logger)
        {
            _httpClientFactory = httpClientFactory;
            _libraryApiTokenProvider = libraryApiTokenProvider;
        }

        public override async ValueTask<HttpClient> CreateHttpClientAsync(List<KeyValuePair<string, string>> extraHeaders = null)
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(200); //TODO: Technical debth, add this value to config

            var bearerToken = await _libraryApiTokenProvider.GetBearerTokenForLibraryApiOnBehalfOfCurrentUserAsync();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            if (extraHeaders != null)
            {
                extraHeaders.ForEach(h => httpClient.DefaultRequestHeaders.Add(h.Key, h.Value));
            }

            return httpClient;
        }
    }
}
