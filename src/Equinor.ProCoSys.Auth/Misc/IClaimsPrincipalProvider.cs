using System.Security.Claims;

namespace Equinor.ProCoSys.Auth.Misc
{
    public interface IClaimsPrincipalProvider
    {
        ClaimsPrincipal GetCurrentClaimsPrincipal();
    }
}
