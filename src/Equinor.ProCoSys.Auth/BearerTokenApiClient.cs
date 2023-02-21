using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Auth
{
    public abstract class BearerTokenApiClient
    {
        protected readonly ILogger<BearerTokenApiClient> _logger;

        public BearerTokenApiClient(ILogger<BearerTokenApiClient> logger) => _logger = logger;

        public async Task<T> TryQueryAndDeserializeAsync<T>(string url, List<KeyValuePair<string, string>> extraHeaders = null)
            => await QueryAndDeserializeAsync<T>(url, true, extraHeaders);

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

            var msg = $"{stopWatch.Elapsed.TotalSeconds}s elapsed when requesting '{url}'. Status: {response.StatusCode}";
            if (!response.IsSuccessStatusCode)
            {
                if (tryGet && response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogWarning(msg);
                    return default;
                }
                _logger.LogError(msg);
                throw new Exception($"Requesting '{url}' was unsuccessful. Status={response.StatusCode}");
            }

            _logger.LogInformation(msg);
            var jsonResult = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<T>(jsonResult, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return result;
        }

        public abstract ValueTask<HttpClient> CreateHttpClientAsync(List<KeyValuePair<string, string>> extraHeaders = null);
    }
}
