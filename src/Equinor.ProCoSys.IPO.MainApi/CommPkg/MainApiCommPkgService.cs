using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.MainApi.Client;
using Equinor.ProCoSys.IPO.MainApi.Plant;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.MainApi.CommPkg
{
    public class MainApiCommPkgService : ICommPkgApiService
    {
        private readonly IBearerTokenApiClient _mainApiClient;
        private readonly Uri _baseAddress;
        private readonly IPlantCache _plantCache;
        private readonly string _apiVersion;

        public MainApiCommPkgService(
            IBearerTokenApiClient mainApiClient,
            IOptionsMonitor<MainApiOptions> options,
            IPlantCache plantCache)
        {
            _mainApiClient = mainApiClient;
            _baseAddress = new Uri(options.CurrentValue.BaseAddress);
            _plantCache = plantCache;
            _apiVersion = options.CurrentValue.ApiVersion;
        }

        public async Task<IList<ProCoSysCommPkg>> SearchCommPkgsByCommPkgNoAsync(string plant, int projectId,
            string startsWithCommPkgNo)
        {
            if (!await _plantCache.IsValidPlantForCurrentUserAsync(plant))
            {
                throw new ArgumentException($"Invalid plant: {plant}");
            }

            var items = new List<ProCoSysCommPkg>();

            var url = $"{_baseAddress}CommPkg/Search" +
                      $"?plantId={plant}" +
                      $"&startsWithCommPkgNo={WebUtility.UrlEncode(startsWithCommPkgNo)}" +
                      $"&projectId={projectId}" +
                      $"&api-version={_apiVersion}";

            var commPkgSearchResult = await _mainApiClient.QueryAndDeserializeAsync<ProCoSysCommPkgSearchResult>(url);

            if (commPkgSearchResult?.Items != null && commPkgSearchResult.Items.Any())
            {
                items.AddRange(commPkgSearchResult.Items);
            }
            
            return items;
        }
    }
}
