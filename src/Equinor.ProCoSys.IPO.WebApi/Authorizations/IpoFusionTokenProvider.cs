using System.Threading.Tasks;
using Fusion.Integration.Configuration;
using Microsoft.Identity.Web;

namespace Equinor.ProCoSys.IPO.WebApi.Authorizations;

public class IpoFusionTokenProvider(ITokenAcquisition tokenAcquisition) : IFusionTokenProvider
{
    public async Task<string> GetApplicationTokenAsync(string scope)
    {
        if (DefaultScopeConverterHelper.TryConvertToDefaultScope(scope, out var defaultScope))
        {
            scope = defaultScope;
        }

        return await tokenAcquisition.GetAccessTokenForAppAsync(
            scope,
            tokenAcquisitionOptions: new TokenAcquisitionOptions());
    }

    public async Task<string> GetDelegatedToken(string scope)
    {
        if (DefaultScopeConverterHelper.TryConvertToDefaultScope(scope, out var defaultScope))
        {
            scope = defaultScope;
        }

        return await tokenAcquisition.GetAccessTokenForUserAsync(
            [scope],
            tokenAcquisitionOptions: new TokenAcquisitionOptions());
    }
}
