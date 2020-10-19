using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.ForeignApi.Client;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.Permission
{
    public class MainApiPermissionService : IPermissionApiService
    {
        private readonly string _apiVersion;
        private readonly Uri _baseAddress;
        private readonly IBearerTokenApiClient _foreignApiClient;

        public MainApiPermissionService(
            IBearerTokenApiClient foreignApiClient,
            IOptionsMonitor<MainApiOptions> options)
        {
            _foreignApiClient = foreignApiClient;
            _apiVersion = options.CurrentValue.ApiVersion;
            _baseAddress = new Uri(options.CurrentValue.BaseAddress);
        }
        
        public async Task<IList<ProCoSysProject>> GetAllProjectsAsync(string plantId)
        {
            var url = $"{_baseAddress}Projects" +
                      $"?plantId={plantId}" +
                      "&withCommPkgsOnly=false" +
                      $"&api-version={_apiVersion}";

            return await _foreignApiClient.QueryAndDeserializeAsync<List<ProCoSysProject>>(url) ?? new List<ProCoSysProject>();
        }

        public async Task<IList<string>> GetPermissionsAsync(string plantId)
        {
            var url = $"{_baseAddress}Permissions" +
                      $"?plantId={plantId}" +
                      $"&api-version={_apiVersion}";

            return await _foreignApiClient.QueryAndDeserializeAsync<List<string>>(url) ?? new List<string>();
        }
    }
}
