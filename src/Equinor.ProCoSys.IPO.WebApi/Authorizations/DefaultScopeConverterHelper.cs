using System;

namespace Equinor.ProCoSys.IPO.WebApi.Authorizations;

public static class DefaultScopeConverterHelper
{
    public static bool TryConvertToDefaultScope(string scope, out string defaultScope)
    {
        // Logic is based on DefaultFusionTokenProvider GetDefaultScope function.
        // https://github.com/equinor/fusion-integration-lib/blob/3e44d2899c8b6563f1cf8bdb132dd905b35f7df3/src/Fusion.Integration/Internals/DefaultFusionTokenProvider.cs#L154

        defaultScope = string.Empty;

        if (Guid.TryParse(scope, out var clientId))
        {
            defaultScope = $"{clientId}/.default";
            return true;
        }

        if (Uri.TryCreate(scope, UriKind.Absolute, out var uri))
        {
            defaultScope = new Uri(uri, ".default").ToString();
            return true;
        }

        return false;
    }
}
