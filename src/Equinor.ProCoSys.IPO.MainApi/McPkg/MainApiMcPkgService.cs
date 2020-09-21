using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.MainApi.Client;
using Equinor.ProCoSys.IPO.MainApi.Plant;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.MainApi.McPkg
{
    public class MainApiMcPkgService : IMcPkgApiService
    {
        private readonly IBearerTokenApiClient _mainApiClient;
        private readonly Uri _baseAddress;
        private readonly IPlantCache _plantCache;
        private readonly string _apiVersion;

        public MainApiMcPkgService(
            IBearerTokenApiClient mainApiClient,
            IOptionsMonitor<MainApiOptions> options,
            IPlantCache plantCache)
        {
            _mainApiClient = mainApiClient;
            _baseAddress = new Uri(options.CurrentValue.BaseAddress);
            _plantCache = plantCache;
            _apiVersion = options.CurrentValue.ApiVersion;
        }

        public async Task<IList<ProCoSysMcPkg>> GetMcPkgsByCommPkgNoAndProjectNameAsync(
            string plant, 
            string projectName,
            string commPkgNo)
        {
            if (!await _plantCache.IsValidPlantForCurrentUserAsync(plant))
            {
                throw new ArgumentException($"Invalid plant: {plant}");
            }

            var url = $"{_baseAddress}McPkg/McPkgs" +
                      $"?plantId={plant}" +
                      $"&projectName={WebUtility.UrlEncode(projectName)}" +
                      $"&commPkgNo={WebUtility.UrlEncode(commPkgNo)}" +
                      $"&api-version={_apiVersion}";

            var mcPkgsResult = await _mainApiClient.QueryAndDeserializeAsync<ProCoSysMcPkgSearchResult>(url);

            var mcPkgs = new List<ProCoSysMcPkg>();
            if (mcPkgsResult?.Items != null && mcPkgsResult.Items.Any())
            {
                mcPkgs.AddRange(mcPkgsResult.Items);
            }

            return mcPkgs;
        }
    }
}
