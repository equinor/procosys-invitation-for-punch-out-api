using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Fusion.Integration.Configuration;

namespace Equinor.ProCoSys.IPO.WebApi.Authorizations;

public class IpoFusionTokenProvider(IIpoFusionCredential credential) : IFusionTokenProvider
{
    public async Task<string> GetApplicationTokenAsync(string scope) => await GetAppAccessTokenAsync(scope);

    public async Task<string> GetDelegatedToken(string scope) => await GetAppAccessTokenAsync(scope);
    
    private async Task<string> GetAppAccessTokenAsync(string scope)
    {
        var ipoCredential = await credential.GetCredentialAsync();

        var token = await ipoCredential.GetTokenAsync(new TokenRequestContext([scope]), CancellationToken.None);

        return token.Token;
    }
}
