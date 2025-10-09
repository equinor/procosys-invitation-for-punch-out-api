using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Client;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg
{
    public class MainApiForUserMcPkgService : IMcPkgApiForUserService
    {
        private readonly IMainApiClientForUser _apiClient;
        private readonly Uri _baseAddress;
        private readonly string _apiVersion;

        public MainApiForUserMcPkgService(
            IMainApiClientForUser apiClient,
            IOptionsMonitor<MainApiOptions> options)
        {
            _apiClient = apiClient;
            _baseAddress = new Uri(options.CurrentValue.BaseAddress);
            _apiVersion = options.CurrentValue.ApiVersion;
        }

        public async Task<IList<ProCoSysMcPkgOnCommPkg>> GetMcPkgsByCommPkgNoAndProjectNameAsync(
            string plant,
            string projectName,
            string commPkgNo,
            CancellationToken cancellationToken)
        {
            var url = $"{_baseAddress}CommPkg/McPkgs" +
                      $"?plantId={plant}" +
                      $"&projectName={WebUtility.UrlEncode(projectName)}" +
                      $"&commPkgNo={WebUtility.UrlEncode(commPkgNo)}" +
                      $"&api-version={_apiVersion}";

            var mcPkgs = await _apiClient.QueryAndDeserializeAsync<List<ProCoSysMcPkgOnCommPkg>>(url, cancellationToken);

            return mcPkgs;
        }
        
        public async Task<IList<ProCoSysMcPkg>> GetMcPkgsByMcPkgNosAsync(
            string plant,
            string projectName,
            IList<string> mcPkgNos,
            CancellationToken cancellationToken)
        {
            var baseUrl = $"{_baseAddress}McPkgs/ByMcPkgNos" +
                      $"?plantId={plant}" +
                      $"&projectName={WebUtility.UrlEncode(projectName)}" +
                      $"&api-version={_apiVersion}";
            var mcPkgNosChunks = mcPkgNos.Chunk(80);
            var pcsMcPkgs = new List<ProCoSysMcPkg>();

            foreach (var chunk in mcPkgNosChunks)
            {
                var mcPkgNosString = "";
                foreach (var mcPkgNo in chunk)
                {
                    mcPkgNosString += $"&mcPkgNos={mcPkgNo}";
                }
                var response = await _apiClient.QueryAndDeserializeAsync<List<ProCoSysMcPkg>>(baseUrl + mcPkgNosString, cancellationToken);
                pcsMcPkgs.AddRange(response);
            }

            return pcsMcPkgs;
        }

        
    }
}
