using System.Security.Claims;

namespace Equinor.ProCoSys.IPO.WebApi.Misc
{
    public interface IClaimsProvider
    {
        ClaimsPrincipal GetCurrentUser();
    }
}
