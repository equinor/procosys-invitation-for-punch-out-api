using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Auth.Permission
{
    public class MainApiPlantService : IPlantApiService
    {
        private readonly string _apiVersion;
        private readonly Uri _baseAddress;
        private readonly IMainApiClient _apiClient;

        public MainApiPlantService(
            IMainApiClient apiClient,
            IOptionsMonitor<MainApiOptions> options)
        {
            _apiClient = apiClient;
            _apiVersion = options.CurrentValue.ApiVersion;
            _baseAddress = new Uri(options.CurrentValue.BaseAddress);
        }

        public async Task<List<ProCoSysPlant>> GetAllPlantsAsync()
        {
            var url = $"{_baseAddress}Plants?includePlantsWithoutAccess=true&api-version={_apiVersion}";
            return await _apiClient.QueryAndDeserializeAsync<List<ProCoSysPlant>>(url);
        }
    }
}
