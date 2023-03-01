using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Equinor.ProCoSys.Auth.Misc
{
    /// <summary>
    /// Get ClaimsPrincipal from current HttpContext, if any
    /// </summary>
    public class ClaimsPrincipalProvider : IClaimsPrincipalProvider
    {
        private readonly ClaimsPrincipal _principal;

        public ClaimsPrincipalProvider(IHttpContextAccessor accessor) => _principal = accessor?.HttpContext?.User ?? new ClaimsPrincipal();

        public ClaimsPrincipal GetCurrentClaimsPrincipal() => _principal;
    }
}
