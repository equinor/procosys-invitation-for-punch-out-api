using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Authentication;
using Equinor.ProCoSys.Auth.Client;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Auth.Person
{
    public class MainApiPersonService : IPersonApiService
    {
        private readonly IMainApiTokenProvider _mainApiTokenProvider;
        private readonly Uri _baseAddress;
        private readonly string _apiVersion;
        private readonly IMainApiClient _mainApiClient;

        public MainApiPersonService(
            IMainApiTokenProvider mainApiTokenProvider,
            IMainApiClient mainApiClient,
            IOptionsMonitor<MainApiOptions> options)
        {
            _mainApiTokenProvider = mainApiTokenProvider;
            _mainApiClient = mainApiClient;
            _baseAddress = new Uri(options.CurrentValue.BaseAddress);
            _apiVersion = options.CurrentValue.ApiVersion;
        }

        public async Task<ProCoSysPerson> TryGetPersonByOidAsync(Guid azureOid)
        {
            var url = $"{_baseAddress}Person" +
                      $"?azureOid={azureOid:D}" +
                      $"&api-version={_apiVersion}";

            var oldAuthType = _mainApiTokenProvider.AuthenticationType;
            _mainApiTokenProvider.AuthenticationType = AuthenticationType.AsApplication;
            try
            {
                return await _mainApiClient.TryQueryAndDeserializeAsync<ProCoSysPerson>(url);
            }
            finally
            {
                _mainApiTokenProvider.AuthenticationType = oldAuthType;
            }
        }
    }
}
