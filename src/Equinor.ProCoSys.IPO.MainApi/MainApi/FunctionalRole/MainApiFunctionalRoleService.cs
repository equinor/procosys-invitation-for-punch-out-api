using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.ForeignApi.Client;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.FunctionalRole
{
    public class MainApiFunctionalRoleService : IFunctionalRoleApiService
    {
        private readonly IBearerTokenApiClient _foreignApiClient;
        private readonly Uri _baseAddress;
        private readonly string _apiVersion;

        public MainApiFunctionalRoleService(
            IBearerTokenApiClient foreignApiClient,
            IOptionsMonitor<MainApiOptions> options)
        {
            _foreignApiClient = foreignApiClient;
            _baseAddress = new Uri(options.CurrentValue.BaseAddress);
            _apiVersion = options.CurrentValue.ApiVersion;
        }

        public async Task<IList<string>> GetFunctionalRoleCodesByPersonOidAsync(
            string plant,
            string azureOid)
        {
            var url = $"{_baseAddress}/Library/FunctionalRoleCodesByPersonOid" +
                      $"?plantId={plant}" +
                      $"&personOid={WebUtility.UrlEncode(azureOid)}" +
                      $"&api-version={_apiVersion}";

            var result = await _foreignApiClient.QueryAndDeserializeAsync<List<ProCoSysFunctionalRoleCode>>(url);
            return result.Select(r => r.Code).ToList();
        }
    }
}
