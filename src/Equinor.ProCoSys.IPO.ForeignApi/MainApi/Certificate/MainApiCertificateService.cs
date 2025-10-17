using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Client;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.Certificate
{
    public class MainApiCertificateService : ICertificateApiService
    {
        private readonly string _apiVersion;
        private readonly Uri _baseAddress;
        private readonly IMainApiClientForApplication _mainApiClient;

        public MainApiCertificateService(
            IMainApiClientForApplication mainApiClient,
            IOptionsMonitor<MainApiOptions> options)
        {
            _mainApiClient = mainApiClient;
            _baseAddress = new Uri(options.CurrentValue.BaseAddress);
            _apiVersion = options.CurrentValue.ApiVersion;
        }

        public async Task<PCSCertificateMcPkgsModel> TryGetCertificateMcPkgsAsync(string plant, Guid proCoSysGuid, CancellationToken cancellationToken)
        {
            var url = $"{_baseAddress}Certificate/McPkgsByCertificateGuid" +
                      $"?plantId={plant}" +
                      $"&proCoSysGuid={proCoSysGuid.ToString("N")}" +
                      $"&api-version={_apiVersion}";

            return await _mainApiClient.TryQueryAndDeserializeAsync<PCSCertificateMcPkgsModel>(url, cancellationToken);
        }

        public async Task<PCSCertificateCommPkgsModel> TryGetCertificateCommPkgsAsync(string plant, Guid proCoSysGuid, CancellationToken cancellationToken)
        {
            var url = $"{_baseAddress}Certificate/CommPkgsByCertificateGuid" +
                      $"?plantId={plant}" +
                      $"&proCoSysGuid={proCoSysGuid.ToString("N")}" +
                      $"&api-version={_apiVersion}";

            return await _mainApiClient.TryQueryAndDeserializeAsync<PCSCertificateCommPkgsModel>(url, cancellationToken);
        }
    }
}
