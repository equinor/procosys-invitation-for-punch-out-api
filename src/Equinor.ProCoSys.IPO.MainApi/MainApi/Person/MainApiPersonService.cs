using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.ForeignApi.Client;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person
{
    public class MainApiPersonService : IPersonApiService
    {
        private readonly IBearerTokenApiClient _foreignApiClient;
        private readonly Uri _baseAddress;
        private readonly string _apiVersion;

        public MainApiPersonService(
            IBearerTokenApiClient foreignApiClient,
            IOptionsMonitor<MainApiOptions> options)
        {
            _foreignApiClient = foreignApiClient;
            _baseAddress = new Uri(options.CurrentValue.BaseAddress);
            _apiVersion = options.CurrentValue.ApiVersion;
        }

        public async Task<IList<ProCoSysPerson>> GetPersonsAsync(
            string plant,
            string searchString,
            long numberOfRows = 1000)
        {
            var url = $"{_baseAddress}Person/PersonSearch" +
                      $"?plantId={plant}" +
                      $"&searchString={WebUtility.UrlEncode(searchString)}" +
                      $"&numberOfRows={numberOfRows}" +
                      $"&api-version={_apiVersion}";

            return await _foreignApiClient.QueryAndDeserializeAsync<List<ProCoSysPerson>>(url);
        }

        public async Task<IList<ProCoSysPerson>> GetPersonsWithPrivilegesAsync(
            string plant,
            string searchString,
            string objectName,
            IList<string> privileges)
        {
            var url = $"{_baseAddress}Person/PersonSearch/ByPrivileges" +
                      $"?plantId={plant}" +
                      $"&searchString={WebUtility.UrlEncode(searchString)}" +
                      $"&objectName={WebUtility.UrlEncode(objectName)}" +
                      $"&api-version={_apiVersion}";
            foreach (var privilege in privileges)
            {
                url += $"&privilegeTypes={privilege}";
            }

            return await _foreignApiClient.QueryAndDeserializeAsync<List<ProCoSysPerson>>(url);
        }

        public async Task<IList<ProCoSysPerson>> GetPersonsByOidsAsync(string plant, IList<string> azureOids)
        {
            var url = $"{_baseAddress}Person/PersonsByOids" +
                      $"?plantId={plant}" +
                      $"&api-version={_apiVersion}";
            foreach (var oid in azureOids)
            {
                url += $"&azureOids={oid}";
            }

            return await _foreignApiClient.QueryAndDeserializeAsync<List<ProCoSysPerson>>(url);
        }

        public async Task<ProCoSysPerson> GetPersonByOidWithPrivilegesAsync(
            string plant,
            string azureOid,
            string objectName,
            IList<string> privileges)
        {
            var url = $"{_baseAddress}Person/PersonByOidWithPrivileges" +
                      $"?plantId={plant}" +
                      $"&azureOid={WebUtility.UrlEncode(azureOid)}" +
                      $"&objectName={WebUtility.UrlEncode(objectName)}" +
                      $"&api-version={_apiVersion}";
            foreach (var privilege in privileges)
            {
                url += $"&privileges={privilege}";
            }

            return await _foreignApiClient.QueryAndDeserializeAsync<ProCoSysPerson>(url);
        }

        public async Task<ProCoSysPerson> GetPersonInFunctionalRoleAsync(
            string plant,
            string azureOid,
            string functionalRoleCode)
        {
            var url = $"{_baseAddress}Person/PersonByOidInFunctionalRole" +
                      $"?plantId={plant}" +
                      $"&azureOid={WebUtility.UrlEncode(azureOid)}" +
                      $"&functionalRoleCode={WebUtility.UrlEncode(functionalRoleCode)}" +
                      $"&api-version={_apiVersion}";

            return await _foreignApiClient.QueryAndDeserializeAsync<ProCoSysPerson>(url);
        }

    }
}
