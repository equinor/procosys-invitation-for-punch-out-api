using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Client;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.Me
{
    public class MainApiMeService : IMeApiService
    {
        private readonly IMainApiClient _apiClient;
        private readonly Uri _baseAddress;
        private readonly string _apiVersion;

        public MainApiMeService(
            IMainApiClient apiClient,
            IOptionsMonitor<MainApiOptions> options)
        {
            _apiClient = apiClient;
            _baseAddress = new Uri(options.CurrentValue.BaseAddress);
            _apiVersion = options.CurrentValue.ApiVersion;
        }

        public async Task<IList<string>> GetFunctionalRoleCodesAsync(
            string plant)
        {
            var url = $"{_baseAddress}/Me/FunctionalRoleCodes" +
                      $"?plantId={plant}" +
                      $"&api-version={_apiVersion}";

            var result = await _apiClient.QueryAndDeserializeAsync<List<ProCoSysFunctionalRoleCode>>(url);
            return result.Select(r => r.Code).ToList();
        }
    }
}
