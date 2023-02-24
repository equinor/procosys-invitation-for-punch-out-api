using System.Security.Claims;

namespace Equinor.ProCoSys.IPO.WebApi.Misc
{
    public interface IClaimsPrincipalProvider
    {
        ClaimsPrincipal GetCurrentClaimsPrincipal();
    }
}
