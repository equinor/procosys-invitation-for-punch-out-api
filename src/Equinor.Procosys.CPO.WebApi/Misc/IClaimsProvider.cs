using System.Security.Claims;

namespace Equinor.Procosys.CPO.WebApi.Misc
{
    public interface IClaimsProvider
    {
        ClaimsPrincipal GetCurrentUser();
    }
}
