using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Client;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;

public class MainApiForApplicationMcPkgService : IMcPkgApiForApplicationService
{
    private readonly IMainApiClientForApplication _apiClient;
    private readonly Uri _baseAddress;
    private readonly string _apiVersion;

    public MainApiForApplicationMcPkgService(
        IMainApiClientForApplication apiClient,
        IOptionsMonitor<MainApiOptions> options)
    {
        _apiClient = apiClient;
        _baseAddress = new Uri(options.CurrentValue.BaseAddress);
        _apiVersion = options.CurrentValue.ApiVersion;
    }
    
    public async Task<ProCoSysMcPkg> GetMcPkgByIdAsync(
        string plant,
        long mcPkgId,
        CancellationToken cancellationToken)
    {
        var baseUrl = $"{_baseAddress}McPkg" +
                      $"?plantId={plant}" +
                      $"&mcPkgId={mcPkgId}" +
                      $"&api-version={_apiVersion}";
        var response = await _apiClient.QueryAndDeserializeAsync<ProCoSysMcPkg>(baseUrl, cancellationToken);
        return response;
    }
}
