using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.Project
{
    public class MainApiProjectService : IProjectApiService
    {
        private readonly string _apiVersion;
        private readonly Uri _baseAddress;
        private readonly IMainApiClient _apiClient;

        public MainApiProjectService(
            IMainApiClient apiClient,
            IOptionsMonitor<MainApiOptions> options)
        {
            _apiClient = apiClient;
            _apiVersion = options.CurrentValue.ApiVersion;
            _baseAddress = new Uri(options.CurrentValue.BaseAddress);
        }

        public async Task<ProCoSysProject> TryGetProjectAsync(string plant, string name)
        {
            var url = $"{_baseAddress}ProjectByName" +
                $"?plantId={plant}" +
                $"&projectName={WebUtility.UrlEncode(name)}" +
                $"&api-version={_apiVersion}";

            return await _apiClient.TryQueryAndDeserializeAsync<ProCoSysProject>(url);
        }

        public async Task<IList<ProCoSysProject>> GetProjectsInPlantAsync(string plant)
        {
            var url = $"{_baseAddress}Projects" +
                      $"?plantId={plant}" +
                      $"&api-version={_apiVersion}" +
                      "&includeSubProjectsOnly=true";

            var projects = await _apiClient.QueryAndDeserializeAsync<List<ProCoSysProject>>(url);

            return projects;
        }
    }
}
