using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Client;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg
{
    public class MainApiCommPkgService : ICommPkgApiService
    {
        private readonly IMainApiClient _apiClient;
        private readonly Uri _baseAddress;
        private readonly string _apiVersion;

        public MainApiCommPkgService(
            IMainApiClient apiClient,
            IOptionsMonitor<MainApiOptions> options)
        {
            _apiClient = apiClient;
            _baseAddress = new Uri(options.CurrentValue.BaseAddress);
            _apiVersion = options.CurrentValue.ApiVersion;
        }

        public async Task<ProCoSysCommPkgSearchResult> SearchCommPkgsByCommPkgNoAsync(
            string plant,
            string projectName,
            string startsWithCommPkgNo,
            int? itemsPerPage = 10,
            int? currentPage = 0)
        {
            var url = $"{_baseAddress}CommPkg/Search" +
                      $"?plantId={plant}" +
                      $"&startsWithCommPkgNo={WebUtility.UrlEncode(startsWithCommPkgNo)}" +
                      $"&projectName={WebUtility.UrlEncode(projectName)}" +
                      "&includeVoidedCommPkgs=false" +
                      "&includeDecommissioningPkgs=true" +
                      $"&itemsPerPage={itemsPerPage}" +
                      $"&currentPage={currentPage}" +
                      $"&api-version={_apiVersion}";

            var commPkgSearchResult = await _apiClient.QueryAndDeserializeAsync<ProCoSysCommPkgSearchResult>(url);

            return commPkgSearchResult;
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

            var commPkgs = await _apiClient.QueryAndDeserializeAsync<List<ProCoSysCommPkg>>(url);

            return commPkgs;
        }

        public async Task<IList<ProCoSysRfocOnCommPkg>> GetRfocGuidsByCommPkgNosAsync(
            string plant,
            string projectName,
            IList<string> commPkgNos)
        {
            var baseUrl = $"{_baseAddress}CommPkg/RfocByCommPkgNos" +
                      $"?plantId={plant}" +
                      $"&projectName={WebUtility.UrlEncode(projectName)}" +
                      $"&api-version={_apiVersion}";
            var commPkgNosChunks = commPkgNos.Chunk(80);
            var rfocs = new List<ProCoSysRfocOnCommPkg>();

            foreach (var chunk in commPkgNosChunks)
            {
                var commPkgNosString = "";
                foreach (var commPkgNo in chunk)
                {
                    commPkgNosString += $"&commPkgNos={commPkgNo}";
                }
                var response = await _apiClient.QueryAndDeserializeAsync<List<ProCoSysRfocOnCommPkg>>(baseUrl + commPkgNosString);
                rfocs.AddRange(response);
            }

            return rfocs;
        }
    }
}
