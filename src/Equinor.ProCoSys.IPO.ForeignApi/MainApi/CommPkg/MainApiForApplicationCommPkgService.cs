using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Client;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg
{
    public class MainApiForApplicationCommPkgService : ICommPkgApiForApplicationService
    {
        private readonly IMainApiClientForApplication _apiClient;
        private readonly Uri _baseAddress;
        private readonly string _apiVersion;

        public MainApiForApplicationCommPkgService(
            IMainApiClientForApplication apiClient,
            IOptionsMonitor<MainApiOptions> options)
        {
            _apiClient = apiClient;
            _baseAddress = new Uri(options.CurrentValue.BaseAddress);
            _apiVersion = options.CurrentValue.ApiVersion;
        }

        public async Task<IList<ProCoSysCommPkg>> GetCommPkgsByCommPkgNosAsync(
            string plant,
            string projectName,
            IList<string> commPkgNos,
            CancellationToken cancellationToken)
        {
            var url = $"{_baseAddress}CommPkg/ByCommPkgNos" +
                      $"?plantId={plant}" +
                      $"&projectName={WebUtility.UrlEncode(projectName)}" +
                      $"&api-version={_apiVersion}";
            foreach (var commPkgNo in commPkgNos)
            {
                url += $"&commPkgNos={commPkgNo}";
            }

            var commPkgs = await _apiClient.QueryAndDeserializeAsync<List<ProCoSysCommPkg>>(url, cancellationToken);

            return commPkgs;
        }
    }
}
