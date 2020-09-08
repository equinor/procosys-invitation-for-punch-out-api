using System;
using System.Net;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.MainApi.Client;
using Equinor.ProCoSys.IPO.MainApi.Plant;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.MainApi.TagFunction
{
    public class MainApiTagFunctionService : ITagFunctionApiService
    {
        private readonly string _apiVersion;
        private readonly Uri _baseAddress;
        private readonly IBearerTokenApiClient _mainApiClient;
        private readonly IPlantCache _plantCache;

        public MainApiTagFunctionService(IBearerTokenApiClient mainApiClient,
            IPlantCache plantCache,
            IOptionsMonitor<MainApiOptions> options)
        {
            _mainApiClient = mainApiClient;
            _plantCache = plantCache;
            _apiVersion = options.CurrentValue.ApiVersion;
            _baseAddress = new Uri(options.CurrentValue.BaseAddress);
        }

        public async Task<ProCoSysTagFunction> TryGetTagFunctionAsync(string plant, string tagFunctionCode, string registerCode)
        {
            if (!await _plantCache.IsValidPlantForCurrentUserAsync(plant))
            {
                throw new ArgumentException($"Invalid plant: {plant}");
            }

            var url = $"{_baseAddress}Library/TagFunction" +
                $"?plantId={plant}" +
                $"&tagFunctionCode={WebUtility.UrlEncode(tagFunctionCode)}" +
                $"&registerCode={WebUtility.UrlEncode(registerCode)}" +
                $"&api-version={_apiVersion}";

            return await _mainApiClient.TryQueryAndDeserializeAsync<ProCoSysTagFunction>(url);
        }
    }
}
