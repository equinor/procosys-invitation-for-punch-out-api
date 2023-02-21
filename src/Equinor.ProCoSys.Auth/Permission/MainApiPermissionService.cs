﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Auth.Permission
{
    public class MainApiPermissionService : IPermissionApiService
    {
        private readonly string _apiVersion;
        private readonly Uri _baseAddress;
        private readonly string _clientFriendlyName;
        private readonly IMainApiClient _apiClient;

        public MainApiPermissionService(
            IMainApiClient apiClient,
            IOptionsMonitor<MainApiOptions> options)
        {
            _apiClient = apiClient;
            _apiVersion = options.CurrentValue.ApiVersion;
            _baseAddress = new Uri(options.CurrentValue.BaseAddress);
            _clientFriendlyName = options.CurrentValue.ClientFriendlyName;
        }
        
        public async Task<IList<ProCoSysProject>> GetAllOpenProjectsAsync(string plantId)
        {
            // trace users use of plant each time getting projects
            // this will serve the purpose since we want to log once a day pr user pr plant, and IPO client ALWAYS get projects at startup
            await TracePlantAsync(plantId);

            var url = $"{_baseAddress}Projects" +
                      $"?plantId={plantId}" +
                      "&withCommPkgsOnly=false" +
                      "&includeProjectsWithoutAccess=true" +
                      $"&api-version={_apiVersion}";

            return await _apiClient.QueryAndDeserializeAsync<List<ProCoSysProject>>(url) ?? new List<ProCoSysProject>();
        }

        public async Task<IList<string>> GetPermissionsAsync(string plantId)
        {
            var url = $"{_baseAddress}Permissions" +
                      $"?plantId={plantId}" +
                      $"&api-version={_apiVersion}";

            return await _apiClient.QueryAndDeserializeAsync<List<string>>(url) ?? new List<string>();
        }

        private async Task TracePlantAsync(string plant)
        {
            var url = $"{_baseAddress}Me/TracePlant" +
                      $"?plantId={plant}" +
                      $"&api-version={_apiVersion}";

            var json = JsonSerializer.Serialize(_clientFriendlyName);
            await _apiClient.PostAsync(url, new StringContent(json, Encoding.Default, "application/json"));
        }
    }
}
