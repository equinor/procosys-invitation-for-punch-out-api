namespace Equinor.ProCoSys.IPO.WebApi.Authorizations;

public static class DefaultScopeConverterHelper
{
    public static bool TryConvertToDefaultScope(string scope, out string defaultScope)
    {
        defaultScope = string.Empty;

        return false;
    }
}
