using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

namespace Equinor.ProCoSys.IPO.ForeignApi.Client;

public class LibraryApiForUserTokenHandler(
    IOptionsMonitor<ApplicationOptions> options,
    ITokenAcquisition tokenAcquisition) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var token = await GetTokenForUser(options.CurrentValue.LibraryApiScope, cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await base.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new Exception("There was a problem fetching the LibraryApi bearer token for user", ex);
        }
    }

    private async Task<string> GetTokenForUser(string scope, CancellationToken cancellationToken)
    {
        return await tokenAcquisition.GetAccessTokenForUserAsync(
            new[] { scope },
            tokenAcquisitionOptions: new TokenAcquisitionOptions { CancellationToken = cancellationToken });
    }
}
