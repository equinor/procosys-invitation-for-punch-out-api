using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.ForeignApi.Client;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.Me
{
    public class MainApiMeService : IMeApiService
    {
        private readonly IBearerTokenApiClient _foreignApiClient;
        private readonly Uri _baseAddress;
        private readonly string _apiVersion;

        public MainApiMeService(
            IBearerTokenApiClient foreignApiClient,
            IOptionsMonitor<MainApiOptions> options)
        {
            _foreignApiClient = foreignApiClient;
            _baseAddress = new Uri(options.CurrentValue.BaseAddress);
            _apiVersion = options.CurrentValue.ApiVersion;
        }

        public async Task<IList<string>> GetFunctionalRoleCodesAsync(
            string plant)
        {
            var url = $"{_baseAddress}/Me/FunctionalRoleCodes" +
                      $"?plantId={plant}" +
                      $"&api-version={_apiVersion}";

            var result = await _foreignApiClient.QueryAndDeserializeAsync<List<ProCoSysFunctionalRoleCode>>(url);
            return result.Select(r => r.Code).ToList();
        }

        public async Task TracePlantAsync(string plant)
        {
            var url = $"{_baseAddress}Me/TracePlant" +
                      $"?plantId={plant}" +
                      $"&api-version={_apiVersion}";

            var json = JsonSerializer.Serialize("ProCoSys - IPO");
            await _foreignApiClient.PostAsync(url, new StringContent(json, Encoding.Default, "application/json"));
        }
    }
}
