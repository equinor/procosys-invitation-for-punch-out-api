using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Client;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Auth.Permission
{
    /// <summary>
    /// Service to get permissions for an user. Permissions are read from Main, using  Main Api
    /// </summary>
    public class MainApiPermissionService : IPermissionApiService
    {
        private readonly string _apiVersion;
        private readonly Uri _baseAddress;
        private readonly string _clientFriendlyName;
        private readonly IMainApiClient _mainApiClient;

        public MainApiPermissionService(
            IMainApiClient mainApiClient,
            IOptionsMonitor<MainApiOptions> options)
        {
            _mainApiClient = mainApiClient;
            _apiVersion = options.CurrentValue.ApiVersion;
            _baseAddress = new Uri(options.CurrentValue.BaseAddress);
            _clientFriendlyName = options.CurrentValue.ClientFriendlyName;
        }

        public async Task<List<AccessablePlant>> GetAllPlantsForUserAsync(Guid azureOid)
        {
            var url = $"{_baseAddress}Plants/ForUser" +
                      $"?azureOid={azureOid:D}" +
                      "&includePlantsWithoutAccess=true" +
                      $"&api-version={_apiVersion}";

            return await _mainApiClient.QueryAndDeserializeAsApplicationAsync<List<AccessablePlant>>(url);
        }

        public async Task<List<AccessableProject>> GetAllOpenProjectsForCurrentUserAsync(string plantId)
        {
            // trace users use of plant each time getting projects
            // this will serve the purpose since we want to log once a day pr user pr plant, and ProCoSys clients as Preservation and IPO ALWAYS get projects at startup
            await TracePlantAsync(plantId);

            var url = $"{_baseAddress}Projects" +
                      $"?plantId={plantId}" +
                      "&withCommPkgsOnly=false" +
                      "&includeProjectsWithoutAccess=true" +
                      $"&api-version={_apiVersion}";

            return await _mainApiClient.QueryAndDeserializeAsync<List<AccessableProject>>(url) ?? new List<AccessableProject>();
        }

        public async Task<List<string>> GetPermissionsForCurrentUserAsync(string plantId)
        {
            var url = $"{_baseAddress}Permissions" +
                      $"?plantId={plantId}" +
                      $"&api-version={_apiVersion}";

            return await _mainApiClient.QueryAndDeserializeAsync<List<string>>(url) ?? new List<string>();
        }

        public async Task<List<string>> GetRestrictionRolesForCurrentUserAsync(string plantId)
        {
            var url = $"{_baseAddress}ContentRestrictions" +
                      $"?plantId={plantId}" +
                      $"&api-version={_apiVersion}";

            return await _mainApiClient.QueryAndDeserializeAsync<List<string>>(url) ?? new List<string>();
        }

        private async Task TracePlantAsync(string plant)
        {
            var url = $"{_baseAddress}Me/TracePlant" +
                      $"?plantId={plant}" +
                      $"&api-version={_apiVersion}";

            var json = JsonSerializer.Serialize(_clientFriendlyName);
            await _mainApiClient.PostAsync(url, new StringContent(json, Encoding.Default, "application/json"));
        }
    }
}
