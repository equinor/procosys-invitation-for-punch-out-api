using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.ForeignApi.Client;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg
{
    public class MainApiMcPkgService : IMcPkgApiService
    {
        private readonly IBearerTokenApiClient _foreignApiClient;
        private readonly Uri _baseAddress;
        private readonly string _apiVersion;

        public MainApiMcPkgService(
            IBearerTokenApiClient foreignApiClient,
            IOptionsMonitor<MainApiOptions> options)
        {
            _foreignApiClient = foreignApiClient;
            _baseAddress = new Uri(options.CurrentValue.BaseAddress);
            _apiVersion = options.CurrentValue.ApiVersion;
        }

        public async Task<IList<ProCoSysMcPkg>> GetMcPkgsByCommPkgNoAndProjectNameAsync(
            string plant, 
            string projectName,
            string commPkgNo)
        {
            var url = $"{_baseAddress}McPkgs" +
                      $"?plantId={plant}" +
                      $"&projectName={WebUtility.UrlEncode(projectName)}" +
                      $"&commPkgNo={WebUtility.UrlEncode(commPkgNo)}" +
                      $"&api-version={_apiVersion}";

            var mcPkgs = await _foreignApiClient.QueryAndDeserializeAsync<List<ProCoSysMcPkg>>(url);

            return mcPkgs;
        }

        public async Task<IList<ProCoSysMcPkg>> GetMcPkgsByMcPkgNosAsync(
            string plant,
            string projectName,
            IList<string> mcPkgNos)
        {
            var url = $"{_baseAddress}McPkgs/ByMcPkgNos" +
                      $"?plantId={plant}" +
                      $"&projectName={WebUtility.UrlEncode(projectName)}" +
                      $"&api-version={_apiVersion}";
            foreach (var mcPkgNo in mcPkgNos)
            {
                url += $"&mcPkgNos={mcPkgNo}";
            }
            var mcPkgs = await _foreignApiClient.QueryAndDeserializeAsync<List<ProCoSysMcPkg>>(url);

            return mcPkgs;
        }

        public async Task SetM01DatesAsync(
            string plant,
            int invitationId,
            string projectName,
            IList<string> mcPkgNos,
            IList<string> commPkgNos)
        {
            var url = $"{_baseAddress}McPkgs/SetM01" +
                      $"?plantId={plant}" +
                      $"&api-version={_apiVersion}";
            var bodyPayload = new
            {
                ProjectName = projectName,
                ExternalReference = "IPO-" + invitationId,
                McPkgNos = mcPkgNos,
                CommPkgNos = commPkgNos
            };

            var content = new StringContent(JsonConvert.SerializeObject(bodyPayload), Encoding.UTF8, "application/json");
            await _foreignApiClient.PutAsync(url, content);
        }

        public async Task ClearM01DatesAsync(
            string plant,
            int invitationId,
            string projectName,
            IList<string> mcPkgNos,
            IList<string> commPkgNos)
        {
            var url = $"{_baseAddress}McPkgs/ClearM01" +
                      $"?plantId={plant}" +
                      $"&api-version={_apiVersion}";
            var bodyPayload = new
            {
                ProjectName = projectName,
                ExternalReference = "IPO-" + invitationId,
                McPkgNos = mcPkgNos,
                CommPkgNos = commPkgNos
            };

            var content = new StringContent(JsonConvert.SerializeObject(bodyPayload), Encoding.UTF8, "application/json");
            await _foreignApiClient.PutAsync(url, content);
        }

        public async Task SetM02DatesAsync(
            string plant,
            int invitationId,
            string projectName,
            IList<string> mcPkgNos,
            IList<string> commPkgNos)
        {
            var url = $"{_baseAddress}McPkgs/SetM02" +
                      $"?plantId={plant}" +
                      $"&api-version={_apiVersion}";
            var bodyPayload = new
            {
                ProjectName = projectName,
                ExternalReference = "IPO-" + invitationId,
                McPkgNos = mcPkgNos,
                CommPkgNos = commPkgNos
            };

            var content = new StringContent(JsonConvert.SerializeObject(bodyPayload), Encoding.UTF8, "application/json");
            await _foreignApiClient.PutAsync(url, content);
        }

    }
}
