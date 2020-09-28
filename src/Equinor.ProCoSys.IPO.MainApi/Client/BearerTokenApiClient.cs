﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.IPO.ForeignApi.Client
{
    public class BearerTokenApiClient : IBearerTokenApiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IBearerTokenProvider _bearerTokenProvider;
        private readonly ILogger<BearerTokenApiClient> _logger;

        public BearerTokenApiClient(
            IHttpClientFactory httpClientFactory,
            IBearerTokenProvider bearerTokenProvider,
            ILogger<BearerTokenApiClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _bearerTokenProvider = bearerTokenProvider;
            _logger = logger;
        }

        public async Task<T> TryQueryAndDeserializeAsync<T>(string url)
            => await QueryAndDeserializeAsync<T>(url, true);

        public async Task<T> QueryAndDeserializeAsync<T>(string url, List<KeyValuePair<string, string>> extraHeaders=null)
            => await QueryAndDeserializeAsync<T>(url, false, extraHeaders);

        private async Task<T> QueryAndDeserializeAsync<T>(
            string url,
            bool tryGet,
            List<KeyValuePair<string, string>> extraHeaders = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (url.Length > 2000)
            {
                throw new ArgumentException("url exceed max 2000 characters", nameof(url));
            }

            var httpClient = await CreateHttpClientAsync(extraHeaders);

            var stopWatch = Stopwatch.StartNew();
            var response = await httpClient.GetAsync(url);
            stopWatch.Stop();

            if (!response.IsSuccessStatusCode)
            {
                if (tryGet && response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogWarning($"Requesting '{url}' returned 'Not found' and took {stopWatch.Elapsed.TotalSeconds}s.");
                    return default;
                }
                _logger.LogError($"Requesting '{url}' was unsuccessful and took {stopWatch.Elapsed.TotalSeconds}s.");
                throw new Exception($"Requesting '{url}' was unsuccessful. Status={response.StatusCode}");
            }

            _logger.LogDebug($"Request was successful and took {stopWatch.Elapsed.TotalSeconds}s.");
            var jsonResult = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<T>(jsonResult, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return result;
        }

        public async Task PutAsync(string url, HttpContent content)
        {
            var httpClient = await CreateHttpClientAsync();

            var stopWatch = Stopwatch.StartNew();
            var response = await httpClient.PutAsync(url, content);
            stopWatch.Stop();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Request was unsuccessful and took {stopWatch.Elapsed.TotalSeconds}s.");
                throw new Exception();
            }
        }

        private async ValueTask<HttpClient> CreateHttpClientAsync(List<KeyValuePair<string, string>> extraHeaders = null)
        {
            var httpClient = _httpClientFactory.CreateClient();
            if (extraHeaders == null)
            {
                var bearerToken = await _bearerTokenProvider.GetBearerTokenForMainApiOnBehalfOfCurrentUserAsync();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            }
            else
            {
                var bearerToken = await _bearerTokenProvider.GetBearerTokenForLibraryApiOnBehalfOfCurrentUserAsync();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                extraHeaders.ForEach(h => httpClient.DefaultRequestHeaders.Add(h.Key, h.Value));
            }

            return httpClient;
        }
    }
}
