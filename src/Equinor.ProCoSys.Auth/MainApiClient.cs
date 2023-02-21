using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Auth
{
    public class MainApiClient : BearerTokenApiClient, IMainApiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMainApiTokenProvider _mainApiTokenProvider;

        public MainApiClient(IHttpClientFactory httpClientFactory,
            IMainApiTokenProvider mainApiTokenProvider,
            ILogger<MainApiClient> logger) : base(logger)
        {
            _httpClientFactory = httpClientFactory;
            _mainApiTokenProvider = mainApiTokenProvider;
        }

        public async Task PutAsync(string url, HttpContent content)
        {
            var httpClient = await CreateHttpClientAsync();

            var stopWatch = Stopwatch.StartNew();
            var response = await httpClient.PutAsync(url, content);
            stopWatch.Stop();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Putting to '{url}' was unsuccessful and took {stopWatch.Elapsed.TotalMilliseconds}ms. Status: {response.StatusCode}");
                throw new Exception();
            }
        }

        public async Task PostAsync(string url, HttpContent content)
        {
            var httpClient = await CreateHttpClientAsync();

            var stopWatch = Stopwatch.StartNew();
            var response = await httpClient.PostAsync(url, content);
            stopWatch.Stop();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Posting to '{url}' was unsuccessful and took {stopWatch.Elapsed.TotalMilliseconds}ms. Status: {response.StatusCode}");
                throw new Exception();
            }
        }

        public override async ValueTask<HttpClient> CreateHttpClientAsync(List<KeyValuePair<string, string>> extraHeaders = null)
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(200); //TODO: Technical debth, add this value to config

            var bearerToken = await _mainApiTokenProvider.GetBearerTokenForMainApiOnBehalfOfCurrentUserAsync();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            if (extraHeaders != null)
            {
                extraHeaders.ForEach(h => httpClient.DefaultRequestHeaders.Add(h.Key, h.Value));
            }

            return httpClient;
        }
    }
}
