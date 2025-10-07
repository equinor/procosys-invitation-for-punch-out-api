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
    
    public async Task<IList<ProCoSysMcPkg>> GetMcPkgsByMcPkgNosAsync(
        string plant,
        string projectName,
        IList<string> mcPkgNos,
        CancellationToken cancellationToken)
    {
        var baseUrl = $"{_baseAddress}McPkgs/ByMcPkgNos" +
                      $"?plantId={plant}" +
                      $"&projectName={WebUtility.UrlEncode(projectName)}" +
                      $"&api-version={_apiVersion}";
        var mcPkgNosChunks = mcPkgNos.Chunk(80);
        var pcsMcPkgs = new List<ProCoSysMcPkg>();

        foreach (var chunk in mcPkgNosChunks)
        {
            var mcPkgNosString = "";
            foreach (var mcPkgNo in chunk)
            {
                mcPkgNosString += $"&mcPkgNos={mcPkgNo}";
            }
            var response = await _apiClient.QueryAndDeserializeAsync<List<ProCoSysMcPkg>>(baseUrl + mcPkgNosString, cancellationToken);
            pcsMcPkgs.AddRange(response);
        }

        return pcsMcPkgs;
    }
    
    public async Task SetM01DatesAsync(
        string plant,
        int invitationId,
        string projectName,
        IList<string> mcPkgNos,
        IList<string> commPkgNos,
        CancellationToken cancellationToken)
    {
        var url = $"{_baseAddress}McPkgs/SetM01" +
                  $"?plantId={plant}" +
                  $"&api-version={_apiVersion}";
        var bodyPayload = new
        {
            ProjectName = projectName,
            ExternalReference = "IPO-" + invitationId,
            McPkgNos = mcPkgNos,
            CommPkgNos = commPkgNos
        };

        var content = new StringContent(JsonConvert.SerializeObject(bodyPayload), Encoding.UTF8, "application/json");
        await _apiClient.PutAsync(url, content, cancellationToken);
    }
    
    public async Task ClearM01DatesAsync(
            string plant,
            int? invitationId,
            string projectName,
            IList<string> mcPkgNos,
            IList<string> commPkgNos,
            CancellationToken cancellationToken)
        {
            var url = $"{_baseAddress}McPkgs/ClearM01" +
                      $"?plantId={plant}" +
                      $"&api-version={_apiVersion}";

            // Elisabeth sin forklaring: External reference er det som blir vist i "Certificate no" - kolonnen på mc pakke hvis vi har satt M01 og / eller M02
            // datoene fra IPO. ExternalReference er en ny kolonne på mc pakke som ble opprettet mtp IPO, men kalte den noe litt nøytralt for hvis den skal
            // bli brukt til noe annet senere.
            // Om vi sender external referanse eller ikke(evnt null), avhenger av om vi ønsker å ha en referanse til
            // IPOen i "Certificate no" - kolonnen på mc pakke.Hvis vi kansellerer en IPO som har blitt completed, så ønsker Kristen at vi
            // fjerner datoen på M01, men at referansen til IPOen enda henger igjen.Hvis vi unaccepter / uncompelter, så ønsker vi å fjerne
            // alle "sporene" av IPOen, derfor klarer vi referansen også i disse tilfellene.
            // If IPO is completede we will not 
            // TODO: We have to remove this confusing logic around sending/not sending invitationId based on what should happen in main...
            string externalRef = null;
            if (invitationId != null)
            {
                externalRef = "IPO-" + invitationId;
            }

            var bodyPayload = new
            {
                ProjectName = projectName,
                McPkgNos = mcPkgNos,
                CommPkgNos = commPkgNos,
                ExternalReference = externalRef
            };

            var content = new StringContent(JsonConvert.SerializeObject(bodyPayload), Encoding.UTF8, "application/json");
            await _apiClient.PutAsync(url, content, cancellationToken);
        }

        public async Task SetM02DatesAsync(
            string plant,
            int invitationId,
            string projectName,
            IList<string> mcPkgNos,
            IList<string> commPkgNos,
            CancellationToken cancellationToken)
        {
            var url = $"{_baseAddress}McPkgs/SetM02" +
                      $"?plantId={plant}" +
                      $"&api-version={_apiVersion}";
            var bodyPayload = new
            {
                ProjectName = projectName,
                ExternalReference = "IPO-" + invitationId,
                McPkgNos = mcPkgNos,
                CommPkgNos = commPkgNos
            };

            var content = new StringContent(JsonConvert.SerializeObject(bodyPayload), Encoding.UTF8, "application/json");
            await _apiClient.PutAsync(url, content, cancellationToken);
        }

        public async Task ClearM02DatesAsync(
            string plant,
            int invitationId,
            string projectName,
            IList<string> mcPkgNos,
            IList<string> commPkgNos,
            CancellationToken cancellationToken)
        {
            var url = $"{_baseAddress}McPkgs/ClearM02" +
                      $"?plantId={plant}" +
                      $"&api-version={_apiVersion}";
            var bodyPayload = new
            {
                ProjectName = projectName,
                McPkgNos = mcPkgNos,
                CommPkgNos = commPkgNos
            };

            var content = new StringContent(JsonConvert.SerializeObject(bodyPayload), Encoding.UTF8, "application/json");
            await _apiClient.PutAsync(url, content, cancellationToken);
        }
}
