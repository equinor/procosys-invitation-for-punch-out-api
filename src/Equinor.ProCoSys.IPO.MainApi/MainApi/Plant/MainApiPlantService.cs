using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.ForeignApi.Client;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.ForeignApi.Plant
{
    public class MainApiPlantService : IPlantApiService
    {
        private readonly string _apiVersion;
        private readonly Uri _baseAddress;
        private readonly IBearerTokenApiClient _foreignApiClient;

        public MainApiPlantService(
            IBearerTokenApiClient foreignApiClient,
            IOptionsMonitor<MainApiOptions> options)
        {
            _foreignApiClient = foreignApiClient;
            _apiVersion = options.CurrentValue.ApiVersion;
            _baseAddress = new Uri(options.CurrentValue.BaseAddress);
        }

        public async Task<List<ProCoSysPlant>> GetPlantsAsync()
        {
            var url = $"{_baseAddress}Plants?api-version={_apiVersion}";
            return await _foreignApiClient.QueryAndDeserializeAsync<List<ProCoSysPlant>>(url);
        }
    }
}
