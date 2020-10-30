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

            var persons = await _foreignApiClient.QueryAndDeserializeAsync<List<ProCoSysPerson>>(url);

            return persons;
        }

        public async Task<IList<ProCoSysPerson>> GetPersonsByUserGroupAsync(
            string plant,
            string searchString,
            string userGroup)
        {
            var url = $"{_baseAddress}Person/PersonSearch/ByUserGroup" +
                      $"?plantId={plant}" +
                      $"&searchString={WebUtility.UrlEncode(searchString)}" +
                      $"&userGroup={WebUtility.UrlEncode(userGroup)}" +
                      $"&api-version={_apiVersion}";

            var persons = await _foreignApiClient.QueryAndDeserializeAsync<List<ProCoSysPerson>>(url);

            return persons;
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

            var persons = await _foreignApiClient.QueryAndDeserializeAsync<List<ProCoSysPerson>>(url);

            return persons;
        }

        public async Task<ProCoSysPerson> GetPersonByOidsInUserGroupAsync(string plant, string azureOid, string userGroup)
        {
            var url = $"{_baseAddress}Person/PersonSearch/ByUserGroup" +
                      $"?plantId={plant}" +
                      $"&azureOid={WebUtility.UrlEncode(azureOid)}" +
                      $"&userGroup={WebUtility.UrlEncode(userGroup)}" +
                      $"&api-version={_apiVersion}";

            var person = await _foreignApiClient.QueryAndDeserializeAsync<ProCoSysPerson>(url);

            return person;
        }
    }
}
