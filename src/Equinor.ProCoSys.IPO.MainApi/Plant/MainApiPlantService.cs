using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.MainApi.Client;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.MainApi.Plant
{
    public class MainApiPlantService : IPlantApiService
    {
        private readonly string _apiVersion;
        private readonly Uri _baseAddress;
        private readonly IBearerTokenApiClient _mainApiClient;

        public MainApiPlantService(
            IBearerTokenApiClient mainApiClient,
            IOptionsMonitor<MainApiOptions> options)
        {
            _mainApiClient = mainApiClient;
            _apiVersion = options.CurrentValue.ApiVersion;
            _baseAddress = new Uri(options.CurrentValue.BaseAddress);
        }

        public async Task<List<ProCoSysPlant>> GetPlantsAsync()
        {
            var url = $"{_baseAddress}Plants?api-version={_apiVersion}";
            return await _mainApiClient.QueryAndDeserializeAsync<List<ProCoSysPlant>>(url);
        }
    }
}
