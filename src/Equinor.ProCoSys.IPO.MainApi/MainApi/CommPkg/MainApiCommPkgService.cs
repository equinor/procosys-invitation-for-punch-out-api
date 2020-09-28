using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.ForeignApi.Client;
using Equinor.ProCoSys.IPO.ForeignApi.Plant;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg
{
    public class MainApiCommPkgService : ICommPkgApiService
    {
        private readonly IBearerTokenApiClient _foreignApiClient;
        private readonly Uri _baseAddress;
        private readonly IPlantCache _plantCache;
        private readonly string _apiVersion;

        public MainApiCommPkgService(
            IBearerTokenApiClient foreignApiClient,
            IOptionsMonitor<MainApiOptions> options,
            IPlantCache plantCache)
        {
            _foreignApiClient = foreignApiClient;
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

            var projects = new List<ProCoSysCommPkg>();

            var url = $"{_baseAddress}CommPkg/Search" +
                      $"?plantId={plant}" +
                      $"&startsWithCommPkgNo={WebUtility.UrlEncode(startsWithCommPkgNo)}" +
                      $"&projectId={projectId}" +
                      $"&api-version={_apiVersion}";

            var commPkgSearchResult = await _foreignApiClient.QueryAndDeserializeAsync<ProCoSysCommPkgSearchResult>(url);

            if (commPkgSearchResult?.Items != null && commPkgSearchResult.Items.Any())
            {
                projects.AddRange(commPkgSearchResult.Items);
            }
            
            return projects;
        }
    }
}
