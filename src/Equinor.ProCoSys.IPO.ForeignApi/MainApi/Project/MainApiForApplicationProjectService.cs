using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Client;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.Project
{
    public class MainApiForApplicationProjectService : IProjectApiForApplicationService
    {
        private readonly string _apiVersion;
        private readonly Uri _baseAddress;
        private readonly IMainApiClientForApplication _apiClient;

        public MainApiForApplicationProjectService(
            IMainApiClientForApplication apiClient,
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
    }
}
