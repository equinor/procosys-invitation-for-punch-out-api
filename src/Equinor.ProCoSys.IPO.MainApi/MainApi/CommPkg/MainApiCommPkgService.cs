using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.ForeignApi.Client;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg
{
    public class MainApiCommPkgService : ICommPkgApiService
    {
        private readonly IBearerTokenApiClient _foreignApiClient;
        private readonly Uri _baseAddress;
        private readonly string _apiVersion;

        public MainApiCommPkgService(
            IBearerTokenApiClient foreignApiClient,
            IOptionsMonitor<MainApiOptions> options)
        {
            _foreignApiClient = foreignApiClient;
            _baseAddress = new Uri(options.CurrentValue.BaseAddress);
            _apiVersion = options.CurrentValue.ApiVersion;
        }

        public async Task<IList<ProCoSysCommPkg>> SearchCommPkgsByCommPkgNoAsync(string plant, string projectName,
            string startsWithCommPkgNo)
        {
            var commPkgs = new List<ProCoSysCommPkg>();

            var url = $"{_baseAddress}CommPkg/Search" +
                      $"?plantId={plant}" +
                      $"&startsWithCommPkgNo={WebUtility.UrlEncode(startsWithCommPkgNo)}" +
                      $"&projectName={WebUtility.UrlEncode(projectName)}" +
                      $"&api-version={_apiVersion}";

            var commPkgSearchResult = await _foreignApiClient.QueryAndDeserializeAsync<ProCoSysCommPkgSearchResult>(url);

            if (commPkgSearchResult?.Items != null && commPkgSearchResult.Items.Any())
            {
                commPkgs.AddRange(commPkgSearchResult.Items);
            }
            
            return commPkgs;
        }

        public async Task<IList<ProCoSysCommPkg>> GetCommPkgsByCommPkgNosAsync(
            string plant, 
            string projectName,
            IList<string> commPkgNos)
        {
            var url = $"{_baseAddress}CommPkg/ByCommPkgNos" +
                      $"?plantId={plant}" +
                      $"&projectName={WebUtility.UrlEncode(projectName)}" +
                      $"&api-version={_apiVersion}";
            foreach (var commPkgNo in commPkgNos)
            {
                url += $"&commPkgNos={commPkgNo}";
            }

            var commPkgs = await _foreignApiClient.QueryAndDeserializeAsync<List<ProCoSysCommPkg>>(url);

            return commPkgs;
        }
    }
}
