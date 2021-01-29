using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.ForeignApi.Client;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.Project
{
    public class MainApiProjectService : IProjectApiService
    {
        private readonly string _apiVersion;
        private readonly Uri _baseAddress;
        private readonly IBearerTokenApiClient _foreignApiClient;

        public MainApiProjectService(
            IBearerTokenApiClient foreignApiClient,
            IOptionsMonitor<MainApiOptions> options)
        {
            _foreignApiClient = foreignApiClient;
            _apiVersion = options.CurrentValue.ApiVersion;
            _baseAddress = new Uri(options.CurrentValue.BaseAddress);
        }

        public async Task<ProCoSysProject> TryGetProjectAsync(string plant, string name)
        {
            var url = $"{_baseAddress}ProjectByName" +
                $"?plantId={plant}" +
                $"&projectName={WebUtility.UrlEncode(name)}" +
                $"&api-version={_apiVersion}";

            return await _foreignApiClient.TryQueryAndDeserializeAsync<ProCoSysProject>(url);
        }

        public async Task<IList<ProCoSysProject>> GetProjectsInPlantAsync(string plant)
        {
            var url = $"{_baseAddress}Projects" +
                      $"?plantId={plant}" +
                      $"&api-version={_apiVersion}" +
                      "&includeInstallations=false" + // Todo Delete this line and comment below after MainApi 4.46 is released. It's OK to use "unknown" params to an endpoint
                      "&includeSubProjectsOnly=true";  // Use 4.46 param

            var projects = await _foreignApiClient.QueryAndDeserializeAsync<List<ProCoSysProject>>(url);

            return projects;
        }
    }
}
