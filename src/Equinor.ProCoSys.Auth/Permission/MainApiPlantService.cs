using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Auth.Permission
{
    public class MainApiPlantService : IPlantApiService
    {
        private readonly IMainApiTokenProvider _mainApiTokenProvider;
        private readonly string _apiVersion;
        private readonly Uri _baseAddress;
        private readonly IMainApiClient _mainApiClient;

        public MainApiPlantService(
            IMainApiTokenProvider mainApiTokenProvider,
            IMainApiClient mainApiClient,
            IOptionsMonitor<MainApiOptions> options)
        {
            _mainApiTokenProvider = mainApiTokenProvider;
            _mainApiClient = mainApiClient;
            _apiVersion = options.CurrentValue.ApiVersion;
            _baseAddress = new Uri(options.CurrentValue.BaseAddress);
        }

        public async Task<List<ProCoSysPlant>> GetAllPlantsForUserAsync(Guid azureOid)
        {
            var url = $"{_baseAddress}Plants/ForUser" +
                      $"?azureOid={azureOid:D}" +
                      "&includePlantsWithoutAccess=true" +
                      $"&api-version={_apiVersion}";

            var oldAuthType = _mainApiTokenProvider.AuthenticationType;
            _mainApiTokenProvider.AuthenticationType = AuthenticationType.AsApplication;
            try
            {
                return await _mainApiClient.QueryAndDeserializeAsync<List<ProCoSysPlant>>(url);
            }
            finally
            {
                _mainApiTokenProvider.AuthenticationType = oldAuthType;
            }
        }
    }
}
