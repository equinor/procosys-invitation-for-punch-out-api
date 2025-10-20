using System.Threading.Tasks;
using Fusion.Integration.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;

namespace Equinor.ProCoSys.IPO.WebApi.Authorizations;

public class IpoFusionTokenProvider : IFusionTokenProvider
{
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly string _scope;

    public IpoFusionTokenProvider(ITokenAcquisition tokenAcquisition, IConfiguration config)
    {
        _tokenAcquisition = tokenAcquisition;

        var azureAdConfig = config.GetSection("AzureAd");
        var clientId = azureAdConfig.GetValue<string>("ClientId");
        
        _scope = $"{clientId}/.default";
        
    }

    public async Task<string> GetApplicationTokenAsync(string _)
    {
        return await _tokenAcquisition.GetAccessTokenForAppAsync(
            _scope,
            tokenAcquisitionOptions: new TokenAcquisitionOptions());
    }

    public async Task<string> GetDelegatedToken(string _) =>
        await _tokenAcquisition.GetAccessTokenForUserAsync(
            [_scope],
            tokenAcquisitionOptions: new TokenAcquisitionOptions());
}
