using System;

namespace Equinor.ProCoSys.IPO.WebApi.Authorizations;

public static class DefaultScopeConverterHelper
{
    public static bool TryConvertToDefaultScope(string scope, out string defaultScope)
    {
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
