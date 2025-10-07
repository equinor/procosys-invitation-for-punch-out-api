using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Client;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.Project
{
    public class MainApiForUsersProjectService : IProjectApiForUsersService
    {
        private readonly string _apiVersion;
        private readonly Uri _baseAddress;
        private readonly IMainApiClientForUser _apiClient;

        public MainApiForUsersProjectService(
            IMainApiClientForUser apiClient,
            IOptionsMonitor<MainApiOptions> options)
        {
            _apiClient = apiClient;
            _apiVersion = options.CurrentValue.ApiVersion;
            _baseAddress = new Uri(options.CurrentValue.BaseAddress);
        }

        public async Task<ProCoSysProject> TryGetProjectAsync(
            string plant,
            string name,
            CancellationToken cancellationToken)
        {
            var url = $"{_baseAddress}ProjectByName" +
                $"?plantId={plant}" +
                $"&projectName={WebUtility.UrlEncode(name)}" +
                $"&api-version={_apiVersion}";

            return await _apiClient.TryQueryAndDeserializeAsync<ProCoSysProject>(url, cancellationToken);
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
