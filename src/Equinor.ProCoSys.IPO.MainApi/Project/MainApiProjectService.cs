using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.MainApi.Client;
using Equinor.ProCoSys.IPO.MainApi.Plant;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.MainApi.Project
{
    public class MainApiProjectService : IProjectApiService
    {
        private readonly string _apiVersion;
        private readonly Uri _baseAddress;
        private readonly IBearerTokenApiClient _mainApiClient;
        private readonly IPlantCache _plantCache;

        public MainApiProjectService(
            IBearerTokenApiClient mainApiClient,
            IPlantCache plantCache,
            IOptionsMonitor<MainApiOptions> options)
        {
            _mainApiClient = mainApiClient;
            _plantCache = plantCache;
            _apiVersion = options.CurrentValue.ApiVersion;
            _baseAddress = new Uri(options.CurrentValue.BaseAddress);
        }

        public async Task<ProCoSysProject> TryGetProjectAsync(string plant, string name)
        {
            if (!await _plantCache.IsValidPlantForCurrentUserAsync(plant))
            {
                throw new ArgumentException($"Invalid plant: {plant}");
            }

            var url = $"{_baseAddress}ProjectByName" +
                $"?plantId={plant}" +
                $"&projectName={WebUtility.UrlEncode(name)}" +
                $"&api-version={_apiVersion}";

            return await _mainApiClient.TryQueryAndDeserializeAsync<ProCoSysProject>(url);
        }

        public async Task<IList<ProCoSysProject>> GetProjectsInPlantAsync(string plant)
        {
            if (!await _plantCache.IsValidPlantForCurrentUserAsync(plant))
            {
                throw new ArgumentException($"Invalid plant: {plant}");
            }

            var items = new List<ProCoSysProject>();

            var url = $"{_baseAddress}Projects" +
                      $"?plantId={plant}" +
                      $"&api-version={_apiVersion}";

            var projectsResult = await _mainApiClient.QueryAndDeserializeAsync<List<ProCoSysProject>>(url);

            if (projectsResult.Any())
            {
                items.AddRange(projectsResult);
            }

            return items;
        }
    }
}
