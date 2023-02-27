using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Authentication;
using Equinor.ProCoSys.Auth.Client;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Auth.Person
{
    /// <summary>
    /// Service to get Person info from Main, using Main Api
    /// </summary>
    public class MainApiPersonService : IPersonApiService
    {
        private readonly IMainApiAuthenticator _mainApiAuthenticator;
        private readonly Uri _baseAddress;
        private readonly string _apiVersion;
        private readonly IMainApiClient _mainApiClient;

        public MainApiPersonService(
            IMainApiAuthenticator mainApiAuthenticator,
            IMainApiClient mainApiClient,
            IOptionsMonitor<MainApiOptions> options)
        {
            _mainApiAuthenticator = mainApiAuthenticator;
            _mainApiClient = mainApiClient;
            _baseAddress = new Uri(options.CurrentValue.BaseAddress);
            _apiVersion = options.CurrentValue.ApiVersion;
        }

        public async Task<ProCoSysPerson> TryGetPersonByOidAsync(Guid azureOid)
        {
            var url = $"{_baseAddress}Person" +
                      $"?azureOid={azureOid:D}" +
                      $"&api-version={_apiVersion}";

            // Authenticate as application. The Person endpoint in Main Api requires
            // a special role "User.Read.All", which the Azure application registration has
            var oldAuthType = _mainApiAuthenticator.AuthenticationType;
            _mainApiAuthenticator.AuthenticationType = AuthenticationType.AsApplication;
            try
            {
                return await _mainApiClient.TryQueryAndDeserializeAsync<ProCoSysPerson>(url);
            }
            finally
            {
                _mainApiAuthenticator.AuthenticationType = oldAuthType;
            }
        }
    }
}
