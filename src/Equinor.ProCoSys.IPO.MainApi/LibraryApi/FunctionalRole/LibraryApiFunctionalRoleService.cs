using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.ForeignApi.Client;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole
{
    public class LibraryApiFunctionalRoleService : IFunctionalRoleApiService
    {
        private readonly IBearerTokenApiClient _foreignApiClient;
        private readonly Uri _baseAddress;

        public LibraryApiFunctionalRoleService(
            IBearerTokenApiClient foreignApiClient,
            IOptionsMonitor<LibraryApiOptions> options)
        {
            _foreignApiClient = foreignApiClient;
            _baseAddress = new Uri(options.CurrentValue.BaseAddress);
        }

        public async Task<IList<ProCoSysFunctionalRole>> GetFunctionalRolesByClassificationAsync(
            string plant,
            string classification)
        {
            var url = 
                $"{_baseAddress}FunctionalRoles" +
                $"?classification={WebUtility.UrlEncode(classification)}";

            var extraHeaders =
                new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("x-plant", plant)};

            return await _foreignApiClient.QueryAndDeserializeAsync<List<ProCoSysFunctionalRole>>(url, extraHeaders);
        }
    }
}
