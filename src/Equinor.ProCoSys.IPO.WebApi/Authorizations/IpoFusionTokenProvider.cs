using System;
using System.Threading.Tasks;
using Fusion.Integration.Configuration;
using Microsoft.Identity.Web;

namespace Equinor.ProCoSys.IPO.WebApi.Authorizations;

public class IpoFusionTokenProvider(ITokenAcquisition tokenAcquisition) : IFusionTokenProvider
{
    public async Task<string> GetApplicationTokenAsync(string scope)
    {
        if (TryConvertV1ScopeTov2(scope, out var scopeV2))
        {
            scope = scopeV2;
        }
        
        return await tokenAcquisition.GetAccessTokenForAppAsync(
            scope,
            tokenAcquisitionOptions: new TokenAcquisitionOptions());
    }

    public async Task<string> GetDelegatedToken(string scope)
    {
        if (TryConvertV1ScopeTov2(scope, out var scopeV2))
        {
            scope = scopeV2;
        }
        
        return await tokenAcquisition.GetAccessTokenForUserAsync(
            [scope],
            tokenAcquisitionOptions: new TokenAcquisitionOptions());
    }

    private static bool TryConvertV1ScopeTov2(string v1ScopeString, out string v2ScopeString)
    {
        // Logic is based on DefaultFusionTokenProvider GetDefaultScope function.
        // https://github.com/equinor/fusion-integration-lib/blob/3e44d2899c8b6563f1cf8bdb132dd905b35f7df3/src/Fusion.Integration/Internals/DefaultFusionTokenProvider.cs#L154
        
        v2ScopeString = string.Empty;
        
        if (string.IsNullOrWhiteSpace(v1ScopeString))
        {
            return false;
        }
        
        if (Guid.TryParse(v1ScopeString, out var clientId))
        {
            v2ScopeString = $"{clientId}/.default";
            return true;
        }

        if (!Uri.TryCreate(v1ScopeString, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (string.IsNullOrEmpty(uri.PathAndQuery) || uri.PathAndQuery == "/")
        {
            v2ScopeString = new Uri(uri, ".default").ToString();
            return true;
        }

        return false;
    }
}
