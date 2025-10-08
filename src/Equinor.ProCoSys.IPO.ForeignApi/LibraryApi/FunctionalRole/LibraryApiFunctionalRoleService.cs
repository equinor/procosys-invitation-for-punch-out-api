using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.ForeignApi.Client;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole
{
    public class LibraryApiFunctionalRoleService : IFunctionalRoleApiService
    {
        private readonly ILibraryApiForUserClient _apiForUserClient;
        private readonly Uri _baseAddress;

        public LibraryApiFunctionalRoleService(
            ILibraryApiForUserClient apiForUserClient,
            IOptionsMonitor<LibraryApiOptions> options)
        {
            _apiForUserClient = apiForUserClient;
            _baseAddress = new Uri(options.CurrentValue.BaseAddress);
        }

        public async Task<IList<ProCoSysFunctionalRole>> GetFunctionalRolesByClassificationAsync(
            string plant,
            string classification,
            CancellationToken cancellationToken)
        {
            var url =
                $"{_baseAddress}FunctionalRoles" +
                $"?classification={WebUtility.UrlEncode(classification)}";

            var extraHeaders =
                new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("x-plant", plant) };

            return await _apiForUserClient.QueryAndDeserializeAsync<List<ProCoSysFunctionalRole>>(
                url,
                cancellationToken,
                extraHeaders);
        }

        public async Task<IList<ProCoSysFunctionalRole>> GetFunctionalRolesByCodeAsync(
            string plant,
            List<string> functionalRoleCodes)
        {
            var url = $"{_baseAddress}FunctionalRolesByCodes"
                + $"?classification={WebUtility.UrlEncode("IPO")}";

            url = functionalRoleCodes.Aggregate(url, (current, code) => current + $"&functionalRoleCodes={WebUtility.UrlEncode(code)}");

            var extraHeaders =
                new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("x-plant", plant) };

            return await _apiForUserClient.QueryAndDeserializeAsync<List<ProCoSysFunctionalRole>>(url, extraHeaders);
        }
    }
}
