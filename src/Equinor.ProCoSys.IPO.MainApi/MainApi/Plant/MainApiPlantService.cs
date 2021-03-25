using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.ForeignApi.Client;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.Plant
{
    public class MainApiPlantService : IMainPlantApiService
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

        public async Task<List<ProCoSysPlant>> GetAllPlantsAsync()
        {
            var url = $"{_baseAddress}Plants?includePlantsWithoutAccess=true&api-version={_apiVersion}";
            return await _foreignApiClient.QueryAndDeserializeAsync<List<ProCoSysPlant>>(url);
        }
    }
}
